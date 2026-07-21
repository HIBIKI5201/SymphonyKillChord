using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace SinfoniaStudio.NotionMarkdownExporter
{
    /// <summary>
    ///     Enhanced Markdown内の子ページとデータベース参照を解析・書き換えするクラス。
    /// </summary>
    internal static class MarkdownReferenceProcessor
    {
        private static readonly Regex _pagePattern = CreateTagPattern("page");
        private static readonly Regex _databasePattern = CreateTagPattern("database");
        private static readonly Regex _urlAttributePattern = new(
            "\\burl\\s*=\\s*\"(?<url>[^\"]+)\"",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        ///     Markdown内の子ページ参照を取得する。
        /// </summary>
        /// <param name="markdown">Enhanced Markdown。</param>
        /// <returns>重複を除外した子ページ参照。</returns>
        internal static IReadOnlyList<MarkdownReference> FindPageReferences(string markdown)
        {
            return FindReferences(markdown, _pagePattern);
        }

        /// <summary>
        ///     Markdown内のデータベース参照を取得する。
        /// </summary>
        /// <param name="markdown">Enhanced Markdown。</param>
        /// <returns>重複を除外したデータベース参照。</returns>
        internal static IReadOnlyList<MarkdownReference> FindDatabaseReferences(string markdown)
        {
            return FindReferences(markdown, _databasePattern);
        }

        /// <summary>
        ///     子ページとデータベースのNotionタグをローカルMarkdownリンクへ書き換える。
        /// </summary>
        /// <param name="markdown">Enhanced Markdown。</param>
        /// <param name="currentFilePath">現在のMarkdownファイルパス。</param>
        /// <param name="pagePaths">ページIDとファイルパスの対応。</param>
        /// <param name="databasePaths">データベースIDとファイルパスの対応。</param>
        /// <returns>書き換え済みMarkdown。</returns>
        internal static string RewriteReferences(
            string markdown,
            string currentFilePath,
            IReadOnlyDictionary<string, string> pagePaths,
            IReadOnlyDictionary<string, string> databasePaths)
        {
            string rewritten = _pagePattern.Replace(markdown, match =>
                RewriteMatch(match, currentFilePath, pagePaths));
            return _databasePattern.Replace(rewritten, match =>
                RewriteMatch(match, currentFilePath, databasePaths));
        }

        /// <summary>
        ///     指定タグ名に対応するEnhanced Markdownタグの正規表現を生成する。
        /// </summary>
        /// <param name="tagName">タグ名。</param>
        /// <returns>タグを解析する正規表現。</returns>
        private static Regex CreateTagPattern(string tagName)
        {
            return new Regex(
                $@"<{tagName}\b(?<attributes>[^>]*?)(?:>(?<title>.*?)</{tagName}>|\s*/>)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        }

        /// <summary>
        ///     正規表現に一致する参照タグをNotion参照一覧へ変換する。
        /// </summary>
        /// <param name="markdown">Enhanced Markdown。</param>
        /// <param name="pattern">対象タグの正規表現。</param>
        /// <returns>参照一覧。</returns>
        private static IReadOnlyList<MarkdownReference> FindReferences(string markdown, Regex pattern)
        {
            Dictionary<string, MarkdownReference> references = new(StringComparer.OrdinalIgnoreCase);
            foreach (Match match in pattern.Matches(markdown))
            {
                if (!TryReadReference(match, out MarkdownReference? reference) || reference == null) { continue; }
                references.TryAdd(reference.Id, reference);
            }

            return references.Values.ToList();
        }

        /// <summary>
        ///     単一タグの参照先がローカルに存在する場合、Markdownリンクへ変換する。
        /// </summary>
        /// <param name="match">タグの正規表現一致。</param>
        /// <param name="currentFilePath">現在のMarkdownファイルパス。</param>
        /// <param name="paths">参照先IDとファイルパスの対応。</param>
        /// <returns>ローカルリンクまたは元のタグ。</returns>
        private static string RewriteMatch(
            Match match,
            string currentFilePath,
            IReadOnlyDictionary<string, string> paths)
        {
            if (!TryReadReference(match, out MarkdownReference? reference) || reference == null ||
                !paths.TryGetValue(reference.Id, out string? targetPath))
            {
                return match.Value;
            }

            string title = string.IsNullOrWhiteSpace(reference.Title) ? "名称未設定" : reference.Title;
            string relativePath = PathUtility.GetRelativeMarkdownPath(currentFilePath, targetPath);
            return $"[{PathUtility.EscapeLinkText(title)}]({relativePath})";
        }

        /// <summary>
        ///     正規表現一致から参照URL、ID、タイトルを読み取る。
        /// </summary>
        /// <param name="match">タグの正規表現一致。</param>
        /// <param name="reference">読み取った参照。</param>
        /// <returns>読み取りに成功した場合はtrue。</returns>
        private static bool TryReadReference(Match match, out MarkdownReference? reference)
        {
            reference = null;
            Match urlMatch = _urlAttributePattern.Match(match.Groups["attributes"].Value);
            if (!urlMatch.Success) { return false; }

            string url = WebUtility.HtmlDecode(urlMatch.Groups["url"].Value).Trim('{', '}');
            if (!NotionIdentifier.TryExtract(url, out string id)) { return false; }

            string title = WebUtility.HtmlDecode(match.Groups["title"].Value).Trim();
            reference = new MarkdownReference(id, title, url);
            return true;
        }
    }
}
