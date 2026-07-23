using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SinfoniaStudio.NotionMarkdownExporter
{
    /// <summary>
    ///     NotionのURLまたはIDをAPI用UUIDへ変換するクラス。
    /// </summary>
    internal static class NotionIdentifier
    {
        private static readonly Regex _idPattern = new(
            @"(?<![0-9a-fA-F])(?:[0-9a-fA-F]{32}|[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})(?![0-9a-fA-F])",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        ///     URLまたはIDからNotion UUIDを抽出する。
        /// </summary>
        /// <param name="value">URLまたはID。</param>
        /// <param name="id">抽出したUUID。</param>
        /// <returns>抽出に成功した場合はtrue。</returns>
        internal static bool TryExtract(string? value, out string id)
        {
            id = string.Empty;
            if (string.IsNullOrWhiteSpace(value)) { return false; }

            string source = value.Trim().Trim('{', '}', '"', '\'');
            if (Uri.TryCreate(source, UriKind.Absolute, out Uri? uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                source = uri.AbsolutePath;
            }

            Match match = _idPattern.Matches(source).Cast<Match>().LastOrDefault() ?? Match.Empty;
            if (!match.Success || !Guid.TryParse(match.Value, out Guid parsed)) { return false; }

            id = parsed.ToString("D");
            return true;
        }

        /// <summary>
        ///     UUIDをファイル名で利用する短い識別子へ変換する。
        /// </summary>
        /// <param name="id">Notion UUID。</param>
        /// <returns>先頭8文字の識別子。</returns>
        internal static string ToShortId(string id)
        {
            return id.Replace("-", string.Empty, StringComparison.Ordinal)[..8];
        }
    }
}
