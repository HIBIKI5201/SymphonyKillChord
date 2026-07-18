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
        private const int CURRENT_VERSION = 1;

        /// <summary> マニフェスト形式のバージョン。 </summary>
        public int Version { get; set; } = CURRENT_VERSION;

        /// <summary> エクスポート元のルートページID。 </summary>
        public string RootPageId { get; set; } = string.Empty;

        /// <summary> エクスポートを完了したUTC日時。 </summary>
        public DateTimeOffset ExportedAtUtc { get; set; }

        /// <summary> エクスポーターが生成した相対ファイルパス。 </summary>
        public List<string> Files { get; set; } = new();

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
            return JsonSerializer.Deserialize<ExportManifest>(json);
        }

        /// <summary>
        ///     新しいマニフェストを一時ファイル経由で保存する。
        /// </summary>
        /// <param name="outputDirectory">出力先ルート。</param>
        /// <param name="rootPageId">ルートページID。</param>
        /// <param name="generatedFiles">生成した相対ファイルパス。</param>
        internal static async Task SaveAsync(
            string outputDirectory,
            string rootPageId,
            IEnumerable<string> generatedFiles)
        {
            ExportManifest manifest = new()
            {
                RootPageId = rootPageId,
                ExportedAtUtc = DateTimeOffset.UtcNow,
                Files = generatedFiles
                    .Select(NormalizeRelativePath)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
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
        ///     前回マニフェストにのみ存在する管理対象ファイルを削除する。
        /// </summary>
        /// <param name="previous">前回マニフェスト。</param>
        /// <param name="currentRootPageId">今回のルートページID。</param>
        /// <param name="outputDirectory">出力先ルート。</param>
        /// <param name="currentFiles">今回生成したファイル。</param>
        /// <param name="warning">警告出力。</param>
        internal static void DeleteStaleFiles(
            ExportManifest? previous,
            string currentRootPageId,
            string outputDirectory,
            ISet<string> currentFiles,
            Action<string> warning)
        {
            if (previous == null) { return; }
            if (!string.Equals(previous.RootPageId, currentRootPageId, StringComparison.OrdinalIgnoreCase))
            {
                warning("出力先の前回ルートページが異なるため、古いファイルは削除しませんでした。");
                return;
            }

            HashSet<string> current = new(
                currentFiles.Select(NormalizeRelativePath),
                StringComparer.OrdinalIgnoreCase);
            foreach (string relativePath in previous.Files.Select(NormalizeRelativePath).Except(current, StringComparer.OrdinalIgnoreCase))
            {
                string fullPath = Path.GetFullPath(Path.Combine(outputDirectory, relativePath));
                if (!PathUtility.IsInsideDirectory(outputDirectory, fullPath))
                {
                    warning($"マニフェスト内の不正なパスを無視しました: {relativePath}");
                    continue;
                }

                if (!File.Exists(fullPath)) { continue; }
                File.Delete(fullPath);
                DeleteEmptyParentDirectories(Path.GetDirectoryName(fullPath), outputDirectory);
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
}
