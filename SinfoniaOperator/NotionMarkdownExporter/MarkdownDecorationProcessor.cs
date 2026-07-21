using System.Text.RegularExpressions;

namespace SinfoniaStudio.NotionMarkdownExporter
{
    /// <summary>
    ///     Enhanced Markdown固有の装飾マーカーを通常のMarkdown向けに整形するクラス。
    /// </summary>
    internal static class MarkdownDecorationProcessor
    {
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

        /// <summary>
        ///     ブロック背景色の装飾マーカーを削除する。
        /// </summary>
        /// <param name="markdown">Enhanced Markdown。</param>
        /// <returns>背景色装飾マーカーを削除したMarkdown。</returns>
        internal static string RemoveBlockBackgroundColors(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                return markdown;
            }

            string withoutEmptyLines = _emptyBlockBackgroundColorLinePattern.Replace(markdown, string.Empty);
            return _blockBackgroundColorSuffixPattern.Replace(withoutEmptyLines, string.Empty);
        }
    }
}
