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
        private readonly StallWatchdog _watchdog;
        private readonly HashSet<string> _reservedPaths = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, PageExportNode> _pagesById = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DatabaseExportNode> _databasesById = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<PageExportNode> _pages = new();
        private readonly List<DatabaseExportNode> _databases = new();
        private readonly object _reservedPathsLock = new();
        private readonly object _warningLock = new();
        private readonly string _stagingRootDirectory;
        private readonly string _stagingDirectory;
        private ExportManifest? _previousManifest;
        private int _warningCount;

        /// <summary>
        ///     Notionエクスポーターを生成する。
        /// </summary>
        /// <param name="apiClient">Notion APIクライアント。</param>
        /// <param name="options">エクスポート設定。</param>
        /// <param name="watchdog">処理の停止を監視するウォッチドッグ。</param>
        internal NotionExporter(NotionApiClient apiClient, ExporterOptions options, StallWatchdog watchdog)
        {
            _apiClient = apiClient;
            _options = options;
            _watchdog = watchdog;
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
            DateTimeOffset exportStartedAtUtc = DateTimeOffset.UtcNow;
            Directory.CreateDirectory(_options.OutputDirectory);
            Directory.CreateDirectory(_stagingDirectory);
            try
            {
                _previousManifest = LoadPreviousManifest();
                Console.WriteLine($"ルートページを取得します: {_options.RootPageId}");
                await BuildPageAsync(_options.RootPageId, _options.OutputDirectory, null);

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

                using AssetDownloader assetDownloader = new(_watchdog);
                foreach (PageExportNode page in _pages)
                {
                    if (!page.IsUpdated)
                    {
                        foreach (string previousFile in page.GeneratedFiles)
                        {
                            generatedFiles.Add(previousFile);
                        }

                        continue;
                    }

                    _watchdog.ReportProgress($"Markdown出力: {page.Title}");
                    string rawMarkdown = await File.ReadAllTextAsync(page.StagingFilePath, Encoding.UTF8);
                    string markdown = CreatePageMarkdown(page, rawMarkdown);
                    rawMarkdown = string.Empty;
                    markdown = MarkdownDecorationProcessor.RemoveBlockBackgroundColors(markdown);
                    markdown = MarkdownReferenceProcessor.RewriteReferences(
                        markdown,
                        page.FilePath,
                        pagePaths,
                        databasePaths);
                    string pageRelativePath = Path.GetRelativePath(_options.OutputDirectory, page.FilePath);
                    generatedFiles.Add(pageRelativePath);
                    page.GeneratedFiles.Add(pageRelativePath);
                    if (_options.DownloadsAssets)
                    {
                        HashSet<string> pageGeneratedFiles = new(StringComparer.OrdinalIgnoreCase);
                        AssetDownloadResult assets = await assetDownloader.DownloadAndRewriteAsync(
                            page.Id,
                            markdown,
                            page.FilePath,
                            _options.OutputDirectory,
                            pageGeneratedFiles,
                            WriteWarning);
                        markdown = assets.Markdown;
                        assetCount += assets.DownloadedCount;
                        foreach (string generatedFile in pageGeneratedFiles)
                        {
                            generatedFiles.Add(generatedFile);
                            page.GeneratedFiles.Add(generatedFile);
                        }
                    }

                    await WriteMarkdownAsync(page.FilePath, markdown);
                    File.Delete(page.StagingFilePath);
                }

                foreach (DatabaseExportNode database in _databases)
                {
                    _watchdog.ReportProgress($"データベース出力: {database.Metadata.Title}");
                    string markdown = CreateDatabaseMarkdown(database);
                    await WriteMarkdownAsync(database.FilePath, markdown);
                    generatedFiles.Add(Path.GetRelativePath(_options.OutputDirectory, database.FilePath));
                }

                _watchdog.ReportProgress("不要ファイルのクリーンアップ");
                ExportManifest.DeleteObsoleteFiles(
                    _previousManifest,
                    generatedFiles,
                    _options.OutputDirectory,
                    WriteWarning);

                _watchdog.ReportProgress("マニフェストの保存");
                await ExportManifest.SaveAsync(
                    _options.OutputDirectory,
                    _options.RootPageId,
                    exportStartedAtUtc,
                    _options.DownloadsAssets,
                    generatedFiles,
                    _pages,
                    _databases);

                int updatedPageCount = _pages.Count(page => page.IsUpdated);

                return new ExportSummary(
                    _pages.Count,
                    _databases.Count,
                    assetCount,
                    updatedPageCount,
                    _pages.Count - updatedPageCount,
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
        ///     子参照は並列に取得するが、同一ページIDへの再入（循環参照や複数箇所からの参照）は
        ///     登録済みノードを即座に返すことで、待機による無限ループを避ける。
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
            lock (_pagesById)
            {
                if (_pagesById.TryGetValue(pageId, out PageExportNode? existing)) { return existing; }
            }

            PageMetadata metadata = knownMetadata ?? await _apiClient.GetPageAsync(pageId);
            ExportedPageManifest? previousPage = FindPreviousPage(metadata.Id);
            string? previousFilePath = TryGetPreviousPagePath(previousPage);
            bool isUpdated = previousPage == null ||
                             previousFilePath == null ||
                             metadata.LastEditedTime == null ||
                             _previousManifest == null ||
                             _previousManifest.DownloadsAssets != _options.DownloadsAssets ||
                             metadata.LastEditedTime > _previousManifest.ExportedAtUtc;
            IReadOnlyList<MarkdownReference> pageReferences;
            IReadOnlyList<MarkdownReference> databaseReferences;
            string stagingFilePath = string.Empty;
            if (isUpdated)
            {
                Console.WriteLine($"  ページ更新: {metadata.Title}");
                MarkdownPayload payload = await _apiClient.GetCompleteMarkdownAsync(pageId);
                foreach (string warning in payload.Warnings) { WriteWarning(warning); }

                pageReferences = MarkdownReferenceProcessor.FindPageReferences(payload.Markdown);
                databaseReferences = MarkdownReferenceProcessor.FindDatabaseReferences(payload.Markdown);
                stagingFilePath = Path.Combine(_stagingDirectory, metadata.Id + ".md");
                await File.WriteAllTextAsync(stagingFilePath, payload.Markdown, new UTF8Encoding(false));
            }
            else
            {
                Console.WriteLine($"  ページ省略: {metadata.Title}");
                pageReferences = previousPage!.PageReferences
                    .Select(reference => reference.CreateReference())
                    .ToList();
                databaseReferences = previousPage.DatabaseReferences
                    .Select(reference => reference.CreateReference())
                    .ToList();
            }

            PageExportNode node;
            string childDirectory;
            lock (_pagesById)
            {
                // 別の並列経路が待機中に同じページを先に登録している場合は、その結果を採用する。
                if (_pagesById.TryGetValue(pageId, out PageExportNode? raceExisting)) { return raceExisting; }

                string filePath;
                if (previousFilePath != null)
                {
                    filePath = previousFilePath;
                    childDirectory = Path.Combine(
                        Path.GetDirectoryName(filePath) ?? parentDirectory,
                        Path.GetFileNameWithoutExtension(filePath));
                    lock (_reservedPathsLock)
                    {
                        _reservedPaths.Add(childDirectory);
                        _reservedPaths.Add(filePath);
                    }
                }
                else
                {
                    string nodeName;
                    lock (_reservedPathsLock)
                    {
                        nodeName = PathUtility.ReserveName(parentDirectory, metadata.Title, metadata.Id, _reservedPaths);
                    }

                    filePath = Path.Combine(parentDirectory, nodeName + ".md");
                    childDirectory = Path.Combine(parentDirectory, nodeName);
                }

                string propertiesMarkdown = PropertyFormatter.CreateMarkdownTable(metadata.Properties);
                node = new(
                    metadata,
                    propertiesMarkdown,
                    stagingFilePath,
                    filePath,
                    childDirectory,
                    isUpdated,
                    pageReferences,
                    databaseReferences);
                if (!isUpdated)
                {
                    node.GeneratedFiles.AddRange(previousPage!.Files);
                }

                _pagesById[metadata.Id] = node;
                _pages.Add(node);
            }

            List<Task> childTasks = new();
            foreach (MarkdownReference pageReference in node.PageReferences)
            {
                childTasks.Add(BuildChildPageAsync(pageReference, childDirectory));
            }

            foreach (MarkdownReference databaseReference in node.DatabaseReferences)
            {
                childTasks.Add(BuildChildDatabaseAsync(databaseReference, childDirectory));
            }

            await Task.WhenAll(childTasks);

            return node;
        }

        /// <summary>
        ///     子ページ参照を取得する。権限不足は警告に変換して無視する。
        /// </summary>
        /// <param name="pageReference">子ページ参照。</param>
        /// <param name="childDirectory">保存先の親ディレクトリ。</param>
        private async Task BuildChildPageAsync(MarkdownReference pageReference, string childDirectory)
        {
            try
            {
                await BuildPageAsync(pageReference.Id, childDirectory, null);
            }
            catch (NotionApiException ex) when (ex.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden)
            {
                WriteWarning($"子ページ「{pageReference.Title}」を権限不足のため取得できませんでした。");
            }
        }

        /// <summary>
        ///     子データベース参照を取得する。権限不足は警告に変換して無視する。
        /// </summary>
        /// <param name="databaseReference">子データベース参照。</param>
        /// <param name="childDirectory">保存先の親ディレクトリ。</param>
        private async Task BuildChildDatabaseAsync(MarkdownReference databaseReference, string childDirectory)
        {
            try
            {
                await BuildDatabaseAsync(databaseReference.Id, childDirectory);
            }
            catch (NotionApiException ex) when (ex.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden)
            {
                WriteWarning($"データベース「{databaseReference.Title}」を権限不足のため取得できませんでした。");
            }
        }

        /// <summary>
        ///     データベースのスキーマと全データソース行を収集する。
        /// </summary>
        /// <param name="databaseId">データベースID。</param>
        /// <param name="parentDirectory">保存先の親ディレクトリ。</param>
        private async Task BuildDatabaseAsync(string databaseId, string parentDirectory)
        {
            lock (_databasesById)
            {
                if (_databasesById.ContainsKey(databaseId)) { return; }
            }

            DatabaseMetadata metadata = await _apiClient.GetDatabaseAsync(databaseId);
            Console.WriteLine($"  データベース取得: {metadata.Title}");
            string? previousFilePath = TryGetPreviousDatabasePath(metadata.Id);

            DatabaseExportNode node;
            string directoryPath;
            lock (_databasesById)
            {
                if (_databasesById.ContainsKey(databaseId)) { return; }

                string filePath;
                if (previousFilePath != null)
                {
                    filePath = previousFilePath;
                    directoryPath = Path.GetDirectoryName(filePath) ?? parentDirectory;
                    lock (_reservedPathsLock)
                    {
                        _reservedPaths.Add(directoryPath);
                        _reservedPaths.Add(filePath);
                    }
                }
                else
                {
                    string nodeName;
                    lock (_reservedPathsLock)
                    {
                        nodeName = PathUtility.ReserveName(parentDirectory, metadata.Title, metadata.Id, _reservedPaths);
                    }

                    directoryPath = Path.Combine(parentDirectory, nodeName);
                    filePath = Path.Combine(directoryPath, "_database.md");
                }

                node = new(metadata, filePath, directoryPath);
                _databasesById[metadata.Id] = node;
                _databases.Add(node);
            }

            if (metadata.DataSources.Count == 0)
            {
                WriteWarning($"データベース「{metadata.Title}」に取得可能なデータソースがありません。");
                return;
            }

            bool hasMultipleSources = metadata.DataSources.Count > 1;
            IEnumerable<Task> sourceTasks = metadata.DataSources.Select(sourceReference =>
                BuildDataSourceAsync(sourceReference, node, directoryPath, hasMultipleSources));
            await Task.WhenAll(sourceTasks);
        }

        /// <summary>
        ///     データソースのスキーマと全ページを取得し、データベースノードへ登録する。
        /// </summary>
        /// <param name="sourceReference">データソース参照。</param>
        /// <param name="node">登録先のデータベースノード。</param>
        /// <param name="directoryPath">データベースの保存先ディレクトリ。</param>
        /// <param name="hasMultipleSources">データベースが複数データソースを持つかどうか。</param>
        private async Task BuildDataSourceAsync(
            DataSourceReference sourceReference,
            DatabaseExportNode node,
            string directoryPath,
            bool hasMultipleSources)
        {
            try
            {
                DataSourceSchema schema = await _apiClient.GetDataSourceAsync(sourceReference.Id);
                DataSourceExportNode sourceNode = new(sourceReference, schema);
                lock (_databasesById)
                {
                    node.DataSources.Add(sourceNode);
                }

                string pageDirectory = directoryPath;
                if (hasMultipleSources)
                {
                    string sourceName;
                    lock (_reservedPathsLock)
                    {
                        sourceName = PathUtility.ReserveName(
                            directoryPath,
                            sourceReference.Name,
                            sourceReference.Id,
                            _reservedPaths);
                    }

                    pageDirectory = Path.Combine(directoryPath, sourceName);
                }

                List<Task<PageExportNode>> pageTasks = new();
                await foreach (PageMetadata page in _apiClient.QueryDataSourcePagesAsync(sourceReference.Id))
                {
                    pageTasks.Add(BuildPageAsync(page.Id, pageDirectory, page));
                }

                PageExportNode[] pageNodes = await Task.WhenAll(pageTasks);
                foreach (PageExportNode pageNode in pageNodes)
                {
                    if (!sourceNode.Pages.Contains(pageNode)) { sourceNode.Pages.Add(pageNode); }
                }
            }
            catch (NotionApiException ex) when (ex.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden)
            {
                WriteWarning($"データソース「{sourceReference.Name}」を権限不足のため取得できませんでした。");
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
        ///     同じルートページの前回マニフェストからページ情報を取得する。
        /// </summary>
        /// <param name="pageId">NotionページID。</param>
        /// <returns>再利用可能な前回ページ情報。存在しない場合はnull。</returns>
        private ExportedPageManifest? FindPreviousPage(string pageId)
        {
            if (_previousManifest == null ||
                !string.Equals(
                    _previousManifest.RootPageId,
                    _options.RootPageId,
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return _previousManifest.FindPage(pageId);
        }

        /// <summary>
        ///     前回ページのMarkdownが出力先内部に存在する場合、その絶対パスを取得する。
        /// </summary>
        /// <param name="previousPage">前回ページ情報。</param>
        /// <returns>再利用可能なMarkdownパス。存在しない場合はnull。</returns>
        private string? TryGetPreviousPagePath(ExportedPageManifest? previousPage)
        {
            if (previousPage == null || string.IsNullOrWhiteSpace(previousPage.File)) { return null; }

            string filePath = Path.GetFullPath(Path.Combine(_options.OutputDirectory, previousPage.File));
            if (!PathUtility.IsInsideDirectory(_options.OutputDirectory, filePath) || !File.Exists(filePath))
            {
                return null;
            }

            return filePath;
        }

        /// <summary>
        ///     同じルートページの前回マニフェストからデータベースMarkdownの絶対パスを取得する。
        /// </summary>
        /// <param name="databaseId">NotionデータベースID。</param>
        /// <returns>再利用可能なMarkdownパス。存在しない場合はnull。</returns>
        private string? TryGetPreviousDatabasePath(string databaseId)
        {
            if (_previousManifest == null ||
                !string.Equals(
                    _previousManifest.RootPageId,
                    _options.RootPageId,
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            ExportedDatabaseManifest? previousDatabase = _previousManifest.FindDatabase(databaseId);
            if (previousDatabase == null || string.IsNullOrWhiteSpace(previousDatabase.File)) { return null; }

            string filePath = Path.GetFullPath(Path.Combine(_options.OutputDirectory, previousDatabase.File));
            return PathUtility.IsInsideDirectory(_options.OutputDirectory, filePath) ? filePath : null;
        }

        /// <summary>
        ///     警告件数を記録して標準エラー出力へ表示する。
        /// </summary>
        /// <param name="message">警告内容。</param>
        private void WriteWarning(string message)
        {
            lock (_warningLock)
            {
                _warningCount++;
                Console.Error.WriteLine($"  [警告] {message}");
            }
        }
    }
}
