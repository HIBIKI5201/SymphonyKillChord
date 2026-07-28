using System.Text.RegularExpressions;

namespace SinfoniaStudio.SinfoniaOperator
{
    /// <summary>
    ///     Enhanced Markdown APIが返す装飾マーカーやタグを、Discord通知向けの読みやすいテキストへ整形するクラス。
    /// </summary>
    internal static class NotionMarkdownSanitizer
    {
        /// <summary>
        ///     Discord通知用にMarkdownを整形する。
        /// </summary>
        /// <param name="markdown">Enhanced Markdown APIから取得した生のMarkdown。</param>
        /// <returns>通知向けに整形したテキスト。</returns>
        internal static string SanitizeForNotification(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                return markdown;
            }

            string text = _mentionTagPattern.Replace(markdown, ReplaceMentionTag);
            text = _imagePattern.Replace(text, "[画像]");
            text = _selfClosingTagPattern.Replace(text, string.Empty);
            text = _emptyBlockBackgroundColorLinePattern.Replace(text, string.Empty);
            text = _blockBackgroundColorSuffixPattern.Replace(text, string.Empty);
            text = _bareUrlPattern.Replace(text, "<$0>");
            text = _multipleBlankLinesPattern.Replace(text, "\n\n");

            return text.Trim();
        }

        /// <summary>
        ///     mention-page / mention-database タグを、タイトルと埋め込み抑制URLへ書き換える。
        ///     DiscordはMarkdownの[text](url)記法をサポートしておらず、裸のURLは
        ///     自動でリンクプレビュー（埋め込みカード）を展開してしまうため、
        ///     URLを &lt;&gt; で囲んで埋め込みを抑制する。
        /// </summary>
        /// <param name="match">タグの正規表現一致。</param>
        /// <returns>書き換え後の文字列。</returns>
        private static string ReplaceMentionTag(Match match)
        {
            Match urlMatch = _urlAttributePattern.Match(match.Groups["attributes"].Value);
            if (!urlMatch.Success)
            {
                return string.Empty;
            }

            string url = urlMatch.Groups["url"].Value.Trim('{', '}');
            string title = match.Groups["title"].Success ? match.Groups["title"].Value.Trim() : string.Empty;

            return string.IsNullOrEmpty(title) ? $"<{url}>" : $"{title}: <{url}>";
        }

        /// <summary>
        ///     &lt;empty-block/&gt;や&lt;table_of_contents .../&gt;など、
        ///     テキストとして意味を持たない自己終了タグ。&lt;br&gt;は改行として意味があるため除外する。
        /// </summary>
        private static readonly Regex _selfClosingTagPattern = new(
            @"<(?!br\b)[a-z][a-z0-9_\-]*(?:\s[^<>]*)?/>",
            RegexOptions.Compiled |
            RegexOptions.IgnoreCase |
            RegexOptions.CultureInvariant);

        /// <summary>
        ///     &lt;&gt;で囲まれていない裸のURL。壊れたmentionタグの残骸など、
        ///     想定外の形でURLがテキストに残った場合の保険として埋め込みを抑制する。
        /// </summary>
        private static readonly Regex _bareUrlPattern = new(
            @"(?<!<)https?://[^\s<>()]+",
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant);

        private static readonly Regex _mentionTagPattern = new(
            @"<mention-(?:page|database)\b(?<attributes>[^>]*?)(?:>(?<title>.*?)</mention-(?:page|database)>|\s*/>)",
            RegexOptions.Compiled |
            RegexOptions.IgnoreCase |
            RegexOptions.Singleline |
            RegexOptions.CultureInvariant);

        private static readonly Regex _urlAttributePattern = new(
            "\\burl\\s*=\\s*\"(?<url>[^\"]+)\"",
            RegexOptions.Compiled |
            RegexOptions.IgnoreCase |
            RegexOptions.CultureInvariant);

        private static readonly Regex _imagePattern = new(
            @"!\[[^\]]*\]\([^)]*\)",
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant);

        private static readonly Regex _emptyBlockBackgroundColorLinePattern = new(
            @"^[ \t]*(?:\*\*)?\{color=""[a-z]+(?:\\?_bg|_background)""\}(?:\*\*)?[ \t]*(?:\r?\n|$)",
            RegexOptions.Compiled |
            RegexOptions.IgnoreCase |
            RegexOptions.Multiline |
            RegexOptions.CultureInvariant);

        private static readonly Regex _blockBackgroundColorSuffixPattern = new(
            @"[ \t]+(?:\*\*)?\{color=""[a-z]+(?:\\?_bg|_background)""\}(?:\*\*)?[ \t]*(?=\r?$)",
            RegexOptions.Compiled |
            RegexOptions.IgnoreCase |
            RegexOptions.Multiline |
            RegexOptions.CultureInvariant);

        private static readonly Regex _multipleBlankLinesPattern = new(
            @"(\r?\n){3,}",
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant);
    }
}
