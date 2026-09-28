using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SinfoniaStudio.SinfoniaOperator.SpecSearch
{
    /// <summary>
    ///     Exporterのプロパティ表と移行後のキャッシュヘッダから資料の属性を読み取る。
    /// </summary>
    internal static partial class SpecPageMetadataReader
    {
        /// <summary>
        ///     本文とは分離されたページ属性を抽出する。
        /// </summary>
        internal static SpecPageMetadata Read(string markdown, string notionUrl)
        {
            Dictionary<string, string> values = new(StringComparer.Ordinal);
            Match properties = FindProperties(markdown);
            foreach (Match row in PropertyRowRegex().Matches(properties.Value))
            {
                values[row.Groups["name"].Value.Trim()] = row.Groups["value"].Value.Trim();
            }

            string status = Get(values, "仕様状態");
            string implementation = Get(values, "実装状態");
            string legacyStatus = Get(values, "状態");
            if (status.Length == 0 && legacyStatus is "アーカイブ" or "廃止" or "却下" or "検討中" or "保留")
            {
                status = legacyStatus;
            }
            if (implementation.Length == 0 && legacyStatus is "実装済" or "一部実装" or "未実装" or "体験版のみ")
            {
                implementation = legacyStatus;
            }

            Match header = CacheHeaderRegex().Match(markdown);
            string pageId = GetHeaderValue(header.Value, "page_id");
            string lastEdited = GetHeaderValue(header.Value, "last_edited");
            if (lastEdited.Length == 0)
            {
                lastEdited = LastEditedRegex().Match(markdown).Groups["value"].Value.Trim();
            }

            return new SpecPageMetadata(
                pageId.Length > 0 ? pageId : notionUrl,
                Get(values, "文書用途"), status, implementation, Get(values, "適用範囲"), lastEdited);
        }

        /// <summary>
        ///     従来のNotionリンクまたはキャッシュヘッダから正本URLを取得する。
        /// </summary>
        internal static string ReadNotionUrl(string markdown)
        {
            Match header = CacheHeaderRegex().Match(markdown);
            string source = GetHeaderValue(header.Value, "source");
            string id = SpecPageMetadata.NormalizePageId(source);
            if (id.Length == 0) { id = SpecPageMetadata.NormalizePageId(GetHeaderValue(header.Value, "page_id")); }
            if (id.Length > 0) { return $"https://www.notion.so/{id}"; }
            Match link = NotionLinkRegex().Match(markdown);
            return link.Success && IsPreamble(markdown[..link.Index]) ? link.Groups["url"].Value : string.Empty;
        }

        /// <summary>
        ///     属性や自動生成ヘッダを検索本文から取り除く。
        /// </summary>
        internal static string RemoveMetadata(string markdown)
        {
            Match properties = FindProperties(markdown);
            string text = properties.Success ? markdown.Remove(properties.Index, properties.Length) : markdown;
            text = CommentRegex().Replace(text, string.Empty);
            text = NotionLinkRegex().Replace(text, string.Empty);
            return LastEditedRegex().Replace(text, string.Empty);
        }

        /// <summary>
        ///     本文やコード例の表をDB属性として扱わず、Exporterの前置きだけを認識する。
        /// </summary>
        private static Match FindProperties(string markdown)
        {
            Match match = PropertiesRegex().Match(markdown);
            return match.Success && IsPreamble(markdown[..match.Index]) ? match : Match.Empty;
        }

        /// <summary>
        ///     ページ先頭の自動生成ヘッダ・タイトル・出典行だけで構成されるか判定する。
        /// </summary>
        private static bool IsPreamble(string text)
        {
            string remaining = CommentRegex().Replace(text, string.Empty);
            remaining = NotionLinkRegex().Replace(remaining, string.Empty);
            remaining = LastEditedRegex().Replace(remaining, string.Empty);
            if (TitleRegex().Matches(remaining).Count > 1) { return false; }
            return string.IsNullOrWhiteSpace(TitleRegex().Replace(remaining, string.Empty));
        }

        /// <summary>
        ///     未定義のプロパティを空文字として読み取る。
        /// </summary>
        private static string Get(IReadOnlyDictionary<string, string> values, string name)
        {
            return values.TryGetValue(name, out string? value) ? value : string.Empty;
        }

        /// <summary>
        ///     キャッシュヘッダの1行の値を取り出す。
        /// </summary>
        private static string GetHeaderValue(string header, string key)
        {
            Match match = Regex.Match(header, @"(?m)^\s*" + Regex.Escape(key) + @":\s*(?<value>[^\r\n]+)");
            return match.Success ? match.Groups["value"].Value.Trim() : string.Empty;
        }

        /// <summary>
        ///     Exporterの先頭プロパティ表だけを認識する。
        /// </summary>
        [GeneratedRegex(@"(?m)^## プロパティ[ \t]*\r?\n(?:[ \t]*\r?\n|\|[^\r\n]*\r?\n)*(?:---[ \t]*\r?\n)?")]
        private static partial Regex PropertiesRegex();

        /// <summary>
        ///     プロパティ表の名前と値を取り出す。
        /// </summary>
        [GeneratedRegex(@"(?m)^\|\s*(?<name>[^|\r\n]+)\|\s*(?<value>[^\r\n]*?)\|[ \t]*$")]
        private static partial Regex PropertyRowRegex();

        /// <summary>
        ///     ページ先頭の機械可読キャッシュヘッダを認識する。
        /// </summary>
        [GeneratedRegex(@"^\s*<!--[\s\S]*?-->")]
        private static partial Regex CacheHeaderRegex();

        /// <summary>
        ///     HTMLコメントを本文から取り除く。
        /// </summary>
        [GeneratedRegex(@"<!--[\s\S]*?-->")]
        private static partial Regex CommentRegex();

        /// <summary>
        ///     Exporterの正本リンクを認識する。
        /// </summary>
        [GeneratedRegex(@"(?m)^\[Notionで開く\]\((?<url>https://[^)\s]+)\)[ \t]*\r?$")]
        private static partial Regex NotionLinkRegex();

        /// <summary>
        ///     Exporterのページ更新日時を認識する。
        /// </summary>
        [GeneratedRegex(@"(?m)^最終更新:\s*(?<value>[^\r\n]*)")]
        private static partial Regex LastEditedRegex();

        /// <summary>
        ///     Exporterのページタイトル行を認識する。
        /// </summary>
        [GeneratedRegex(@"(?m)^# [^\r\n]*")]
        private static partial Regex TitleRegex();
    }
}
