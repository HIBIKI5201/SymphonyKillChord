using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SinfoniaStudio.NotionMarkdownExporter
{
    /// <summary>
    ///     前回エクスポートで生成したファイルを安全に管理するマニフェスト。
    /// </summary>
    internal sealed class ExportManifest
    {
        private const string MANIFEST_FILE_NAME = ".notion-export-manifest.json";
        private const int LEGACY_VERSION = 1;
        private const int CURRENT_VERSION = 2;

        /// <summary> マニフェスト形式のバージョン。 </summary>
        public int Version { get; set; } = CURRENT_VERSION;

        /// <summary> エクスポート元のルートページID。 </summary>
        public string RootPageId { get; set; } = string.Empty;

        /// <summary> エクスポートを開始したUTC日時。 </summary>
        public DateTimeOffset ExportedAtUtc { get; set; }

        /// <summary> 添付ファイルを取得する設定だったかどうか。 </summary>
        public bool DownloadsAssets { get; set; }

        /// <summary> エクスポーターが生成した相対ファイルパス。 </summary>
        public List<string> Files { get; set; } = new();

        /// <summary> ページごとの前回エクスポート情報。 </summary>
        public List<ExportedPageManifest> Pages { get; set; } = new();

        /// <summary> データベースごとの出力情報。 </summary>
        public List<ExportedDatabaseManifest> Databases { get; set; } = new();

        /// <summary>
        ///     出力先にある前回マニフェストを読み込む。
        /// </summary>
        /// <param name="outputDirectory">出力先ルート。</param>
        /// <returns>前回マニフェスト。存在しない場合はnull。</returns>
        internal static ExportManifest? Load(string outputDirectory)
        {
            string path = Path.Combine(outputDirectory, MANIFEST_FILE_NAME);
            if (!File.Exists(path)) { return null; }

            string json = File.ReadAllText(path, Encoding.UTF8);
            ExportManifest? manifest = JsonSerializer.Deserialize<ExportManifest>(json);
            if (manifest == null)
            {
                throw new InvalidDataException("マニフェストの内容が空です。");
            }

            if (manifest.Version is not LEGACY_VERSION and not CURRENT_VERSION)
            {
                throw new InvalidDataException($"未対応のマニフェストバージョンです: {manifest.Version}");
            }

            if (manifest.Files == null)
            {
                throw new InvalidDataException("マニフェストに生成ファイル一覧がありません。");
            }

            manifest.Pages ??= new List<ExportedPageManifest>();
            manifest.Databases ??= new List<ExportedDatabaseManifest>();
            foreach (ExportedPageManifest page in manifest.Pages)
            {
                page.Files ??= new List<string>();
                page.PageReferences ??= new List<ExportedReferenceManifest>();
                page.DatabaseReferences ??= new List<ExportedReferenceManifest>();
            }

            return manifest;
        }

        /// <summary>
        ///     新しいマニフェストを一時ファイル経由で保存する。
        /// </summary>
        /// <param name="outputDirectory">出力先ルート。</param>
        /// <param name="rootPageId">ルートページID。</param>
        /// <param name="exportedAtUtc">差分比較の基準となるエクスポート開始UTC日時。</param>
        /// <param name="downloadsAssets">添付ファイルを取得するかどうか。</param>
        /// <param name="generatedFiles">生成した相対ファイルパス。</param>
        /// <param name="pages">エクスポート対象ページ。</param>
        /// <param name="databases">エクスポート対象データベース。</param>
        internal static async Task SaveAsync(
            string outputDirectory,
            string rootPageId,
            DateTimeOffset exportedAtUtc,
            bool downloadsAssets,
            IEnumerable<string> generatedFiles,
            IEnumerable<PageExportNode> pages,
            IEnumerable<DatabaseExportNode> databases)
        {
            ExportManifest manifest = new()
            {
                RootPageId = rootPageId,
                ExportedAtUtc = exportedAtUtc,
                DownloadsAssets = downloadsAssets,
                Files = generatedFiles
                    .Select(NormalizeRelativePath)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                Pages = pages
                    .OrderBy(page => page.Id, StringComparer.OrdinalIgnoreCase)
                    .Select(page => ExportedPageManifest.Create(page, outputDirectory))
                    .ToList(),
                Databases = databases
                    .OrderBy(database => database.Metadata.Id, StringComparer.OrdinalIgnoreCase)
                    .Select(database => ExportedDatabaseManifest.Create(database, outputDirectory))
                    .ToList()
            };
            JsonSerializerOptions options = new()
            {
                WriteIndented = true
            };
            string json = JsonSerializer.Serialize(manifest, options);
            string path = Path.Combine(outputDirectory, MANIFEST_FILE_NAME);
            string temporaryPath = path + ".tmp";
            await File.WriteAllTextAsync(temporaryPath, json, new UTF8Encoding(false));
            File.Move(temporaryPath, path, true);
        }

        /// <summary>
        ///     前回マニフェストから指定ページの情報を取得する。
        /// </summary>
        /// <param name="pageId">NotionページID。</param>
        /// <returns>前回のページ情報。存在しない場合はnull。</returns>
        internal ExportedPageManifest? FindPage(string pageId)
        {
            return Pages.FirstOrDefault(page => string.Equals(
                page.Id,
                pageId,
                StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        ///     前回マニフェストから指定データベースの情報を取得する。
        /// </summary>
        /// <param name="databaseId">NotionデータベースID。</param>
        /// <returns>前回のデータベース情報。存在しない場合はnull。</returns>
        internal ExportedDatabaseManifest? FindDatabase(string databaseId)
        {
            return Databases.FirstOrDefault(database => string.Equals(
                database.Id,
                databaseId,
                StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        ///     今回のエクスポート対象から外れた前回生成ファイルだけを削除する。
        /// </summary>
        /// <param name="previous">前回マニフェスト。</param>
        /// <param name="currentFiles">今回の生成ファイル一覧。</param>
        /// <param name="outputDirectory">出力先ルート。</param>
        /// <param name="warning">警告出力。</param>
        internal static void DeleteObsoleteFiles(
            ExportManifest? previous,
            IEnumerable<string> currentFiles,
            string outputDirectory,
            Action<string> warning)
        {
            if (previous == null) { return; }

            HashSet<string> current = new(
                currentFiles.Select(NormalizeRelativePath),
                StringComparer.OrdinalIgnoreCase);
            foreach (string relativePath in previous.Files
                         .Select(NormalizeRelativePath)
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .Where(path => !current.Contains(path)))
            {
                DeleteGeneratedFile(relativePath, outputDirectory, warning);
            }
        }

        /// <summary>
        ///     相対パスの区切り文字を統一する。
        /// </summary>
        /// <param name="path">相対パス。</param>
        /// <returns>正規化した相対パス。</returns>
        private static string NormalizeRelativePath(string path)
        {
            return path.Replace('\\', '/');
        }

        /// <summary>
        ///     出力先内部であることを確認して単一の生成ファイルを削除する。
        /// </summary>
        /// <param name="relativePath">出力先からの相対パス。</param>
        /// <param name="outputDirectory">出力先ルート。</param>
        /// <param name="warning">警告出力。</param>
        private static void DeleteGeneratedFile(
            string relativePath,
            string outputDirectory,
            Action<string> warning)
        {
            string fullPath = Path.GetFullPath(Path.Combine(outputDirectory, relativePath));
            if (!PathUtility.IsInsideDirectory(outputDirectory, fullPath))
            {
                warning($"マニフェスト内の不正なパスを無視しました: {relativePath}");
                return;
            }

            if (!File.Exists(fullPath)) { return; }
            File.Delete(fullPath);
            DeleteEmptyParentDirectories(Path.GetDirectoryName(fullPath), outputDirectory);
        }

        /// <summary>
        ///     出力先ルートへ達するまで空の親ディレクトリを削除する。
        /// </summary>
        /// <param name="directoryPath">削除を開始するディレクトリ。</param>
        /// <param name="outputDirectory">削除対象外の出力先ルート。</param>
        private static void DeleteEmptyParentDirectories(string? directoryPath, string outputDirectory)
        {
            string root = Path.GetFullPath(outputDirectory).TrimEnd(Path.DirectorySeparatorChar);
            string? current = directoryPath;
            while (!string.IsNullOrWhiteSpace(current) &&
                   PathUtility.IsInsideDirectory(outputDirectory, current) &&
                   !string.Equals(Path.GetFullPath(current).TrimEnd(Path.DirectorySeparatorChar), root, StringComparison.OrdinalIgnoreCase) &&
                   Directory.Exists(current) &&
                   !Directory.EnumerateFileSystemEntries(current).Any())
            {
                Directory.Delete(current);
                current = Path.GetDirectoryName(current);
            }
        }
    }

    /// <summary>
    ///     差分エクスポートに必要なページ単位の情報。
    /// </summary>
    internal sealed class ExportedPageManifest
    {
        /// <summary> NotionページID。 </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary> 出力先ルートからのMarkdown相対パス。 </summary>
        public string File { get; set; } = string.Empty;

        /// <summary> ページに属するMarkdownと添付ファイルの相対パス。 </summary>
        public List<string> Files { get; set; } = new();

        /// <summary> 子ページ参照。 </summary>
        public List<ExportedReferenceManifest> PageReferences { get; set; } = new();

        /// <summary> 子データベース参照。 </summary>
        public List<ExportedReferenceManifest> DatabaseReferences { get; set; } = new();

        /// <summary>
        ///     エクスポートノードからマニフェスト情報を生成する。
        /// </summary>
        /// <param name="page">エクスポート対象ページ。</param>
        /// <param name="outputDirectory">出力先ルート。</param>
        /// <returns>保存用ページ情報。</returns>
        internal static ExportedPageManifest Create(PageExportNode page, string outputDirectory)
        {
            return new ExportedPageManifest
            {
                Id = page.Id,
                File = Path.GetRelativePath(outputDirectory, page.FilePath).Replace('\\', '/'),
                Files = page.GeneratedFiles
                    .Select(path => path.Replace('\\', '/'))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                PageReferences = page.PageReferences.Select(ExportedReferenceManifest.Create).ToList(),
                DatabaseReferences = page.DatabaseReferences.Select(ExportedReferenceManifest.Create).ToList()
            };
        }
    }

    /// <summary>
    ///     差分エクスポートで再利用するNotion参照情報。
    /// </summary>
    internal sealed class ExportedReferenceManifest
    {
        /// <summary> 参照先ID。 </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary> 参照先タイトル。 </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary> 参照先URL。 </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        ///     Markdown参照から保存用情報を生成する。
        /// </summary>
        /// <param name="reference">Markdown参照。</param>
        /// <returns>保存用参照情報。</returns>
        internal static ExportedReferenceManifest Create(MarkdownReference reference)
        {
            return new ExportedReferenceManifest
            {
                Id = reference.Id,
                Title = reference.Title,
                Url = reference.Url
            };
        }

        /// <summary>
        ///     保存済み情報をMarkdown参照へ変換する。
        /// </summary>
        /// <returns>Markdown参照。</returns>
        internal MarkdownReference CreateReference()
        {
            return new MarkdownReference(Id, Title, Url);
        }
    }

    /// <summary>
    ///     差分エクスポートでパスを維持するデータベース情報。
    /// </summary>
    internal sealed class ExportedDatabaseManifest
    {
        /// <summary> NotionデータベースID。 </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary> 出力先ルートからのMarkdown相対パス。 </summary>
        public string File { get; set; } = string.Empty;

        /// <summary>
        ///     データベースノードから保存用情報を生成する。
        /// </summary>
        /// <param name="database">エクスポート対象データベース。</param>
        /// <param name="outputDirectory">出力先ルート。</param>
        /// <returns>保存用データベース情報。</returns>
        internal static ExportedDatabaseManifest Create(
            DatabaseExportNode database,
            string outputDirectory)
        {
            return new ExportedDatabaseManifest
            {
                Id = database.Metadata.Id,
                File = Path.GetRelativePath(outputDirectory, database.FilePath).Replace('\\', '/')
            };
        }
    }
}
