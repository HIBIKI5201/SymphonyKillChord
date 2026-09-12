using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SinfoniaStudio.NotionMarkdownExporter;

namespace SinfoniaStudio.NotionMarkdownWriter
{
    /// <summary>
    ///     エクスポート済みMarkdownからページIDと祖先関係を読み取るクラス。
    ///     エクスポーターは各ページを「ページ名.md」と、子を収める「ページ名」フォルダーとして出力するため、
    ///     ディレクトリ構造がそのままNotionの親子関係になる。
    /// </summary>
    internal static class LocalPageLocator
    {
        /// <summary> エクスポーターが各ページ冒頭へ出力する、Notionページへのリンク。 </summary>
        private static readonly Regex _openInNotionPattern = new(
            @"\[Notionで開く\]\((?<url>[^)]+)\)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary> ページIDを探すために読む冒頭行数。 </summary>
        private const int HEADER_SCAN_LINE_COUNT = 10;

        /// <summary>
        ///     指定された文字列を、ローカルパスまたはURL・IDとして解釈しページIDを求める。
        /// </summary>
        /// <param name="value">ローカルのMarkdownパス、NotionのURL、またはID。</param>
        /// <param name="markdownPath">ローカルパスとして解釈できた場合の絶対パス。</param>
        /// <returns>ページID。</returns>
        internal static string ResolvePageId(string value, out string? markdownPath)
        {
            markdownPath = null;
            if (IsMarkdownFilePath(value))
            {
                string fullPath = Path.GetFullPath(value);
                if (!File.Exists(fullPath))
                {
                    throw new WriterException($"Markdownファイルが見つかりません: {fullPath}");
                }

                if (!TryReadPageId(fullPath, out string pageId))
                {
                    throw new WriterException(
                        $"ページIDを読み取れませんでした（[Notionで開く]リンクがありません）: {fullPath}");
                }

                markdownPath = fullPath;
                return pageId;
            }

            if (!NotionIdentifier.TryExtract(value, out string id))
            {
                throw new WriterException($"NotionのURL・ID・Markdownパスとして解釈できません: {value}");
            }

            return id;
        }

        /// <summary>
        ///     エクスポート済みMarkdownの冒頭からページIDを読み取る。
        /// </summary>
        /// <param name="markdownPath">Markdownファイルの絶対パス。</param>
        /// <param name="pageId">読み取ったページID。</param>
        /// <returns>読み取れた場合はtrue。</returns>
        internal static bool TryReadPageId(string markdownPath, out string pageId)
        {
            pageId = string.Empty;
            if (!File.Exists(markdownPath)) { return false; }

            using StreamReader reader = new(markdownPath, Encoding.UTF8);
            for (int index = 0; index < HEADER_SCAN_LINE_COUNT; index++)
            {
                string? line = reader.ReadLine();
                if (line == null) { break; }

                Match match = _openInNotionPattern.Match(line);
                if (!match.Success) { continue; }

                return NotionIdentifier.TryExtract(match.Groups["url"].Value, out pageId);
            }

            return false;
        }

        /// <summary>
        ///     エクスポートのディレクトリ構造から祖先ページIDを近い順に列挙する。
        /// </summary>
        /// <param name="markdownPath">対象Markdownの絶対パス。</param>
        /// <param name="exportDirectory">エクスポート出力先の絶対パス。</param>
        /// <returns>祖先ページID。判定できない場合は空。</returns>
        internal static IReadOnlyList<string> EnumerateAncestorPageIds(string markdownPath, string exportDirectory)
        {
            List<string> ancestors = new();
            string exportRoot = Path.GetFullPath(exportDirectory).TrimEnd(Path.DirectorySeparatorChar);
            string? currentDirectory = Path.GetDirectoryName(Path.GetFullPath(markdownPath));

            while (!string.IsNullOrEmpty(currentDirectory))
            {
                string normalized = Path.GetFullPath(currentDirectory).TrimEnd(Path.DirectorySeparatorChar);
                if (string.Equals(normalized, exportRoot, StringComparison.OrdinalIgnoreCase)) { break; }
                if (!normalized.StartsWith(exportRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                {
                    // エクスポート出力先の外にあるファイルは、ローカル構造から祖先を判定できない。
                    return Array.Empty<string>();
                }

                string parentMarkdownPath = normalized + ".md";
                if (TryReadPageId(parentMarkdownPath, out string parentPageId)) { ancestors.Add(parentPageId); }
                currentDirectory = Path.GetDirectoryName(normalized);
            }

            return ancestors;
        }

        /// <summary>
        ///     引数がMarkdownファイルのパスとして書かれているかを判定する。
        /// </summary>
        /// <param name="value">引数。</param>
        /// <returns>Markdownパスとして扱う場合はtrue。</returns>
        private static bool IsMarkdownFilePath(string value)
        {
            return value.EndsWith(".md", StringComparison.OrdinalIgnoreCase);
        }
    }
}
