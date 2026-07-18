using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SinfoniaStudio.NotionMarkdownExporter
{
    /// <summary>
    ///     Notionのルートページ以下をローカルMarkdownツリーへエクスポートするクラス。
    /// </summary>
    internal sealed class NotionExporter
    {
        private readonly NotionApiClient _apiClient;
        private readonly ExporterOptions _options;
        private readonly HashSet<string> _reservedPaths = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, PageExportNode> _pagesById = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DatabaseExportNode> _databasesById = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<PageExportNode> _pages = new();
        private readonly List<DatabaseExportNode> _databases = new();
        private readonly string _stagingRootDirectory;
        private readonly string _stagingDirectory;
        private int _warningCount;

        /// <summary>
        ///     Notionエクスポーターを生成する。
        /// </summary>
        /// <param name="apiClient">Notion APIクライアント。</param>
        /// <param name="options">エクスポート設定。</param>
        internal NotionExporter(NotionApiClient apiClient, ExporterOptions options)
        {
            _apiClient = apiClient;
            _options = options;
            _stagingRootDirectory = Path.Combine(
                Path.GetTempPath(),
                "SinfoniaStudio",
                "NotionMarkdownExporter");
            _stagingDirectory = Path.Combine(_stagingRootDirectory, Guid.NewGuid().ToString("N"));
        }

        /// <summary>
        ///     ルートページ以下を収集し、Markdownと添付ファイルを出力する。
        /// </summary>
        /// <returns>エクスポート結果。</returns>
        internal async Task<ExportSummary> ExportAsync()
        {
            Directory.CreateDirectory(_options.OutputDirectory);
            Directory.CreateDirectory(_stagingDirectory);
            try
            {
                ExportManifest? previousManifest = LoadPreviousManifest();
                Console.WriteLine($"ルートページを取得します: {_options.RootPageId}");
                await BuildPageAsync(_options.RootPageId, _options.OutputDirectory, null);

                if (previousManifest != null)
                {
                    Console.WriteLine("前回のエクスポートデータをクリーンアップします。");
                    ExportManifest.DeleteGeneratedFiles(
                        previousManifest,
                        _options.OutputDirectory,
                        WriteWarning);
                }

                Dictionary<string, string> pagePaths = _pagesById.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.FilePath,
                    StringComparer.OrdinalIgnoreCase);
                Dictionary<string, string> databasePaths = _databasesById.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.FilePath,
                    StringComparer.OrdinalIgnoreCase);
                HashSet<string> generatedFiles = new(StringComparer.OrdinalIgnoreCase);
                int assetCount = 0;

                using AssetDownloader assetDownloader = new();
                foreach (PageExportNode page in _pages)
                {
                    string rawMarkdown = await File.ReadAllTextAsync(page.StagingFilePath, Encoding.UTF8);
                    string markdown = CreatePageMarkdown(page, rawMarkdown);
                    rawMarkdown = string.Empty;
                    markdown = MarkdownReferenceProcessor.RewriteReferences(
                        markdown,
                        page.FilePath,
                        pagePaths,
                        databasePaths);
                    if (_options.DownloadsAssets)
                    {
                        AssetDownloadResult assets = await assetDownloader.DownloadAndRewriteAsync(
                            page.Id,
                            markdown,
                            page.FilePath,
                            _options.OutputDirectory,
                            generatedFiles,
                            WriteWarning);
                        markdown = assets.Markdown;
                        assetCount += assets.DownloadedCount;
                    }

                    await WriteMarkdownAsync(page.FilePath, markdown);
                    File.Delete(page.StagingFilePath);
                    generatedFiles.Add(Path.GetRelativePath(_options.OutputDirectory, page.FilePath));
                }

                foreach (DatabaseExportNode database in _databases)
                {
                    string markdown = CreateDatabaseMarkdown(database);
                    await WriteMarkdownAsync(database.FilePath, markdown);
                    generatedFiles.Add(Path.GetRelativePath(_options.OutputDirectory, database.FilePath));
                }

                await ExportManifest.SaveAsync(_options.OutputDirectory, _options.RootPageId, generatedFiles);

                return new ExportSummary(
                    _pages.Count,
                    _databases.Count,
                    assetCount,
                    _warningCount,
                    _options.OutputDirectory);
            }
            finally
            {
                DeleteStagingDirectory();
            }
        }

        /// <summary>
        ///     ページを取得して保存先を割り当て、子ページとデータベースを再帰的に収集する。
        /// </summary>
        /// <param name="pageId">ページID。</param>
        /// <param name="parentDirectory">保存先の親ディレクトリ。</param>
        /// <param name="knownMetadata">データソースクエリ等で取得済みのメタデータ。</param>
        /// <returns>ページのエクスポート情報。</returns>
        private async Task<PageExportNode> BuildPageAsync(
            string pageId,
            string parentDirectory,
            PageMetadata? knownMetadata)
        {
            if (_pagesById.TryGetValue(pageId, out PageExportNode? existing)) { return existing; }

            PageMetadata metadata = knownMetadata ?? await _apiClient.GetPageAsync(pageId);
            Console.WriteLine($"  ページ取得: {metadata.Title}");
            MarkdownPayload payload = await _apiClient.GetCompleteMarkdownAsync(pageId);
            foreach (string warning in payload.Warnings) { WriteWarning(warning); }

            IReadOnlyList<MarkdownReference> pageReferences =
                MarkdownReferenceProcessor.FindPageReferences(payload.Markdown);
            IReadOnlyList<MarkdownReference> databaseReferences =
                MarkdownReferenceProcessor.FindDatabaseReferences(payload.Markdown);
            string stagingFilePath = Path.Combine(_stagingDirectory, metadata.Id + ".md");
            await File.WriteAllTextAsync(stagingFilePath, payload.Markdown, new UTF8Encoding(false));

            string nodeName = PathUtility.ReserveName(parentDirectory, metadata.Title, metadata.Id, _reservedPaths);
            string filePath = Path.Combine(parentDirectory, nodeName + ".md");
            string childDirectory = Path.Combine(parentDirectory, nodeName);
            string propertiesMarkdown = PropertyFormatter.CreateMarkdownTable(metadata.Properties);
            PageExportNode node = new(metadata, propertiesMarkdown, stagingFilePath, filePath, childDirectory);
            _pagesById[metadata.Id] = node;
            _pages.Add(node);
            payload = null!;
            metadata = null!;

            foreach (MarkdownReference pageReference in pageReferences)
            {
                if (_pagesById.ContainsKey(pageReference.Id)) { continue; }

                try
                {
                    await BuildPageAsync(pageReference.Id, childDirectory, null);
                }
                catch (NotionApiException ex) when (ex.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden)
                {
                    WriteWarning($"子ページ「{pageReference.Title}」を権限不足のため取得できませんでした。");
                }
            }

            foreach (MarkdownReference databaseReference in databaseReferences)
            {
                if (_databasesById.ContainsKey(databaseReference.Id)) { continue; }

                try
                {
                    await BuildDatabaseAsync(databaseReference.Id, childDirectory);
                }
                catch (NotionApiException ex) when (ex.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden)
                {
                    WriteWarning($"データベース「{databaseReference.Title}」を権限不足のため取得できませんでした。");
                }
            }

            return node;
        }

        /// <summary>
        ///     データベースのスキーマと全データソース行を収集する。
        /// </summary>
        /// <param name="databaseId">データベースID。</param>
        /// <param name="parentDirectory">保存先の親ディレクトリ。</param>
        private async Task BuildDatabaseAsync(string databaseId, string parentDirectory)
        {
            if (_databasesById.ContainsKey(databaseId)) { return; }

            DatabaseMetadata metadata = await _apiClient.GetDatabaseAsync(databaseId);
            Console.WriteLine($"  データベース取得: {metadata.Title}");
            string nodeName = PathUtility.ReserveName(parentDirectory, metadata.Title, metadata.Id, _reservedPaths);
            string directoryPath = Path.Combine(parentDirectory, nodeName);
            string filePath = Path.Combine(directoryPath, "_database.md");
            DatabaseExportNode node = new(metadata, filePath, directoryPath);
            _databasesById[metadata.Id] = node;
            _databases.Add(node);

            if (metadata.DataSources.Count == 0)
            {
                WriteWarning($"データベース「{metadata.Title}」に取得可能なデータソースがありません。");
                return;
            }

            bool hasMultipleSources = metadata.DataSources.Count > 1;
            foreach (DataSourceReference sourceReference in metadata.DataSources)
            {
                try
                {
                    DataSourceSchema schema = await _apiClient.GetDataSourceAsync(sourceReference.Id);
                    DataSourceExportNode sourceNode = new(sourceReference, schema);
                    node.DataSources.Add(sourceNode);

                    string pageDirectory = directoryPath;
                    if (hasMultipleSources)
                    {
                        string sourceName = PathUtility.ReserveName(
                            directoryPath,
                            sourceReference.Name,
                            sourceReference.Id,
                            _reservedPaths);
                        pageDirectory = Path.Combine(directoryPath, sourceName);
                    }

                    await foreach (PageMetadata page in _apiClient.QueryDataSourcePagesAsync(sourceReference.Id))
                    {
                        PageExportNode pageNode = await BuildPageAsync(page.Id, pageDirectory, page);
                        if (!sourceNode.Pages.Contains(pageNode)) { sourceNode.Pages.Add(pageNode); }
                    }
                }
                catch (NotionApiException ex) when (ex.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden)
                {
                    WriteWarning($"データソース「{sourceReference.Name}」を権限不足のため取得できませんでした。");
                }
            }
        }

        /// <summary>
        ///     ページのメタデータ、プロパティ、Enhanced Markdown本文を一つのファイルへ構成する。
        /// </summary>
        /// <param name="page">ページのエクスポート情報。</param>
        /// <param name="rawMarkdown">一時ファイルから読み込んだEnhanced Markdown本文。</param>
        /// <returns>保存するMarkdown。</returns>
        private static string CreatePageMarkdown(PageExportNode page, string rawMarkdown)
        {
            StringBuilder markdown = new();
            markdown.AppendLine("<!-- NotionMarkdownExporterによる自動生成ファイルです。 -->");
            markdown.Append("# ");
            markdown.AppendLine(PathUtility.EscapeHeading(page.Title));
            markdown.AppendLine();
            if (!string.IsNullOrWhiteSpace(page.Url))
            {
                markdown.Append("[Notionで開く](");
                markdown.Append(page.Url);
                markdown.AppendLine(")");
            }

            if (page.LastEditedTime != null)
            {
                markdown.AppendLine();
                markdown.Append("最終更新: ");
                markdown.AppendLine(page.LastEditedTime.Value.ToString("O"));
            }

            if (!string.IsNullOrWhiteSpace(page.PropertiesMarkdown))
            {
                markdown.AppendLine();
                markdown.AppendLine("## プロパティ");
                markdown.AppendLine();
                markdown.AppendLine(page.PropertiesMarkdown);
            }

            if (!string.IsNullOrWhiteSpace(rawMarkdown))
            {
                markdown.AppendLine();
                markdown.AppendLine("---");
                markdown.AppendLine();
                markdown.Append(rawMarkdown.TrimEnd());
                markdown.AppendLine();
            }

            return markdown.ToString();
        }

        /// <summary>
        ///     データベースのスキーマと行ページへの索引をMarkdownへ構成する。
        /// </summary>
        /// <param name="database">データベースのエクスポート情報。</param>
        /// <returns>保存するMarkdown。</returns>
        private static string CreateDatabaseMarkdown(DatabaseExportNode database)
        {
            StringBuilder markdown = new();
            markdown.AppendLine("<!-- NotionMarkdownExporterによる自動生成ファイルです。 -->");
            markdown.Append("# ");
            markdown.AppendLine(PathUtility.EscapeHeading(database.Metadata.Title));
            if (!string.IsNullOrWhiteSpace(database.Metadata.Url))
            {
                markdown.AppendLine();
                markdown.Append("[Notionで開く](");
                markdown.Append(database.Metadata.Url);
                markdown.AppendLine(")");
            }

            foreach (DataSourceExportNode source in database.DataSources)
            {
                markdown.AppendLine();
                markdown.Append("## ");
                markdown.AppendLine(PathUtility.EscapeHeading(source.Reference.Name));
                markdown.AppendLine();
                markdown.AppendLine("### スキーマ");
                markdown.AppendLine();
                markdown.AppendLine("| プロパティ | 型 |");
                markdown.AppendLine("|---|---|");
                foreach (SchemaProperty property in source.Schema.Properties.OrderBy(
                             property => property.Name,
                             StringComparer.OrdinalIgnoreCase))
                {
                    markdown.Append("| ");
                    markdown.Append(property.Name.Replace("|", "\\|", StringComparison.Ordinal));
                    markdown.Append(" | ");
                    markdown.Append(property.Type);
                    markdown.AppendLine(" |");
                }

                markdown.AppendLine();
                markdown.AppendLine("### ページ");
                markdown.AppendLine();
                if (source.Pages.Count == 0)
                {
                    markdown.AppendLine("（ページなし）");
                    continue;
                }

                foreach (PageExportNode page in source.Pages.OrderBy(
                             page => page.Title,
                             StringComparer.OrdinalIgnoreCase))
                {
                    string relativePath = PathUtility.GetRelativeMarkdownPath(database.FilePath, page.FilePath);
                    markdown.Append("- [");
                    markdown.Append(PathUtility.EscapeLinkText(page.Title));
                    markdown.Append("](");
                    markdown.Append(relativePath);
                    markdown.AppendLine(")");
                }
            }

            return markdown.ToString();
        }

        /// <summary>
        ///     MarkdownファイルをUTF-8（BOMなし）で保存する。
        /// </summary>
        /// <param name="path">保存先パス。</param>
        /// <param name="markdown">Markdown本文。</param>
        private static async Task WriteMarkdownAsync(string path, string markdown)
        {
            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory)) { Directory.CreateDirectory(directory); }
            await File.WriteAllTextAsync(path, markdown, new UTF8Encoding(false));
        }

        /// <summary>
        ///     今回の実行で作成した一時ディレクトリを安全性確認後に削除する。
        /// </summary>
        private void DeleteStagingDirectory()
        {
            try
            {
                if (!Directory.Exists(_stagingDirectory)) { return; }
                if (!PathUtility.IsInsideDirectory(_stagingRootDirectory, _stagingDirectory))
                {
                    WriteWarning($"一時ディレクトリが想定範囲外のため削除しませんでした: {_stagingDirectory}");
                    return;
                }

                Directory.Delete(_stagingDirectory, true);
            }
            catch (Exception ex)
            {
                WriteWarning($"一時ディレクトリを削除できませんでした: {ex.Message}");
            }
        }

        /// <summary>
        ///     前回マニフェストを読み込み、破損時は出力の混在を防ぐため処理を停止する。
        /// </summary>
        /// <returns>前回マニフェスト。読み込めない場合はnull。</returns>
        private ExportManifest? LoadPreviousManifest()
        {
            try
            {
                return ExportManifest.Load(_options.OutputDirectory);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException(
                    $"前回マニフェストを読み込めないため、安全にクリーンアップできません: {ex.Message}",
                    ex);
            }
        }

        /// <summary>
        ///     警告件数を記録して標準エラー出力へ表示する。
        /// </summary>
        /// <param name="message">警告内容。</param>
        private void WriteWarning(string message)
        {
            _warningCount++;
            Console.Error.WriteLine($"  [警告] {message}");
        }
    }
}
