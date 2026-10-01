using System;
using SinfoniaStudio.NotionMarkdownExporter;

namespace SinfoniaStudio.NotionMarkdownWriter
{
    /// <summary>
    ///     ブロックを指す引数（URLまたはID）を解決するクラス。
    ///     NotionでブロックへのリンクをコピーするとURLの末尾が「#&lt;ブロックID&gt;」になるため、
    ///     ページ用のNotionIdentifierとは別に、フラグメントを優先して読む。
    /// </summary>
    internal static class BlockReferenceResolver
    {
        /// <summary>
        ///     引数からブロックIDを求める。
        /// </summary>
        /// <param name="value">NotionのブロックURL（#フラグメント付き）またはブロックID。</param>
        /// <returns>ブロックID。</returns>
        internal static string Resolve(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new WriterException("ブロックのURLまたはIDを指定してください。");
            }

            string trimmed = value.Trim();
            if (Uri.TryCreate(trimmed, UriKind.Absolute, out Uri? uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
                !string.IsNullOrEmpty(uri.Fragment) &&
                NotionIdentifier.TryExtract(uri.Fragment.TrimStart('#'), out string fromFragment))
            {
                return fromFragment;
            }

            if (NotionIdentifier.TryExtract(trimmed, out string id)) { return id; }

            throw new WriterException($"NotionのブロックURLまたはIDとして解釈できません: {value}");
        }
    }
}
