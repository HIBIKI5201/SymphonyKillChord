using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SinfoniaStudio.NotionMarkdownExporter
{
    /// <summary>
    ///     エクスポート先の安全なファイル名とリンクパスを生成するクラス。
    /// </summary>
    internal static class PathUtility
    {
        private const int MAX_NAME_LENGTH = 80;

        private static readonly HashSet<string> _reservedWindowsNames = new(
            new[]
            {
                "CON", "PRN", "AUX", "NUL",
                "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
                "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
            },
            StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     同じ親ディレクトリ内で重複しない安全な名前を予約する。
        /// </summary>
        /// <param name="parentDirectory">親ディレクトリ。</param>
        /// <param name="title">元のタイトル。</param>
        /// <param name="id">重複時に使用するNotion ID。</param>
        /// <param name="reservedPaths">予約済みパス。</param>
        /// <returns>拡張子を含まない安全な名前。</returns>
        internal static string ReserveName(
            string parentDirectory,
            string title,
            string id,
            ISet<string> reservedPaths)
        {
            string safeName = CreateSafeName(title);
            string candidate = safeName;
            string candidatePath = Path.Combine(parentDirectory, candidate);
            if (!TryReservePath(candidatePath, reservedPaths))
            {
                candidate = $"{safeName}-{NotionIdentifier.ToShortId(id)}";
                candidatePath = Path.Combine(parentDirectory, candidate);
                int suffix = 2;
                while (!TryReservePath(candidatePath, reservedPaths))
                {
                    candidate = $"{safeName}-{NotionIdentifier.ToShortId(id)}-{suffix++}";
                    candidatePath = Path.Combine(parentDirectory, candidate);
                }
            }

            return candidate;
        }

        /// <summary>
        ///     タイトルをWindowsで使用可能なファイル名へ変換する。
        /// </summary>
        /// <param name="value">元のタイトル。</param>
        /// <returns>安全なファイル名。</returns>
        internal static string CreateSafeName(string value)
        {
            HashSet<char> invalidCharacters = new(Path.GetInvalidFileNameChars());
            char[] characters = value.Trim()
                .Select(character => invalidCharacters.Contains(character) || char.IsControl(character) ? '_' : character)
                .ToArray();
            string result = new string(characters).Trim().TrimEnd('.', ' ');
            while (result.Contains("__", StringComparison.Ordinal))
            {
                result = result.Replace("__", "_", StringComparison.Ordinal);
            }

            if (string.IsNullOrWhiteSpace(result)) { result = "名称未設定"; }
            if (_reservedWindowsNames.Contains(result)) { result = $"_{result}"; }
            if (result.Length > MAX_NAME_LENGTH) { result = result[..MAX_NAME_LENGTH].TrimEnd('.', ' '); }
            return result;
        }

        /// <summary>
        ///     現在のMarkdownファイルから対象ファイルへの相対リンクを生成する。
        /// </summary>
        /// <param name="currentFilePath">現在のMarkdownファイル。</param>
        /// <param name="targetPath">リンク対象ファイル。</param>
        /// <returns>URLエンコード済みの相対パス。</returns>
        internal static string GetRelativeMarkdownPath(string currentFilePath, string targetPath)
        {
            string currentDirectory = Path.GetDirectoryName(currentFilePath) ?? Directory.GetCurrentDirectory();
            string relativePath = Path.GetRelativePath(currentDirectory, targetPath).Replace('\\', '/');
            return string.Join("/", relativePath.Split('/').Select(segment =>
                segment == ".." || segment == "." ? segment : Uri.EscapeDataString(segment)));
        }

        /// <summary>
        ///     Markdownリンクの表示文字列をエスケープする。
        /// </summary>
        /// <param name="value">表示文字列。</param>
        /// <returns>エスケープ済み文字列。</returns>
        internal static string EscapeLinkText(string value)
        {
            return value.Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("[", "\\[", StringComparison.Ordinal)
                .Replace("]", "\\]", StringComparison.Ordinal)
                .Replace("\r", " ", StringComparison.Ordinal)
                .Replace("\n", " ", StringComparison.Ordinal);
        }

        /// <summary>
        ///     Markdown見出しへ埋め込むタイトルから改行を除去する。
        /// </summary>
        /// <param name="value">見出し文字列。</param>
        /// <returns>安全な見出し文字列。</returns>
        internal static string EscapeHeading(string value)
        {
            return value.Replace("\r", " ", StringComparison.Ordinal)
                .Replace("\n", " ", StringComparison.Ordinal)
                .Trim();
        }

        /// <summary>
        ///     指定パスが出力先ディレクトリの内部にあることを確認する。
        /// </summary>
        /// <param name="outputDirectory">出力先ルート。</param>
        /// <param name="candidatePath">確認するパス。</param>
        /// <returns>出力先内部の場合はtrue。</returns>
        internal static bool IsInsideDirectory(string outputDirectory, string candidatePath)
        {
            string root = Path.GetFullPath(outputDirectory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string candidate = Path.GetFullPath(candidatePath);
            return candidate.StartsWith(root, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     同名ディレクトリとMarkdownファイルの両方が衝突しないようパスを予約する。
        /// </summary>
        /// <param name="basePath">拡張子を含まない候補パス。</param>
        /// <param name="reservedPaths">予約済みパス。</param>
        /// <returns>両方の予約に成功した場合はtrue。</returns>
        private static bool TryReservePath(string basePath, ISet<string> reservedPaths)
        {
            string markdownPath = basePath + ".md";
            if (reservedPaths.Contains(basePath) || reservedPaths.Contains(markdownPath)) { return false; }

            reservedPaths.Add(basePath);
            reservedPaths.Add(markdownPath);
            return true;
        }
    }
}
