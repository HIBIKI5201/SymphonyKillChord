using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace SinfoniaStudio.NotionMarkdownExporter
{
    /// <summary>
    ///     Notionページのプロパティを人が読めるMarkdownへ変換するクラス。
    /// </summary>
    internal static class PropertyFormatter
    {
        /// <summary>
        ///     ページプロパティからタイトル値を探す。
        /// </summary>
        /// <param name="properties">ページプロパティ。</param>
        /// <returns>ページタイトル。見つからない場合は空文字。</returns>
        internal static string FindTitle(JsonElement properties)
        {
            if (properties.ValueKind != JsonValueKind.Object) { return string.Empty; }

            foreach (JsonProperty property in properties.EnumerateObject())
            {
                if (!property.Value.TryGetProperty("type", out JsonElement typeElement) ||
                    typeElement.GetString() != "title" ||
                    !property.Value.TryGetProperty("title", out JsonElement titleElement))
                {
                    continue;
                }

                return ReadRichText(titleElement);
            }

            return string.Empty;
        }

        /// <summary>
        ///     Notionのリッチテキスト配列から表示文字列を取得する。
        /// </summary>
        /// <param name="richText">リッチテキスト配列。</param>
        /// <returns>連結した表示文字列。</returns>
        internal static string ReadRichText(JsonElement richText)
        {
            if (richText.ValueKind != JsonValueKind.Array) { return string.Empty; }

            StringBuilder result = new();
            foreach (JsonElement item in richText.EnumerateArray())
            {
                if (item.TryGetProperty("plain_text", out JsonElement plainTextElement))
                {
                    result.Append(plainTextElement.GetString());
                    continue;
                }

                if (item.TryGetProperty("text", out JsonElement textElement) &&
                    textElement.TryGetProperty("content", out JsonElement contentElement))
                {
                    result.Append(contentElement.GetString());
                }
            }

            return result.ToString();
        }

        /// <summary>
        ///     タイトル以外のページプロパティをMarkdownテーブルへ変換する。
        /// </summary>
        /// <param name="properties">ページプロパティ。</param>
        /// <returns>プロパティが無い場合は空文字、それ以外はMarkdownテーブル。</returns>
        internal static string CreateMarkdownTable(JsonElement properties)
        {
            if (properties.ValueKind != JsonValueKind.Object) { return string.Empty; }

            List<(string Name, string Value)> rows = new();
            foreach (JsonProperty property in properties.EnumerateObject())
            {
                string type = property.Value.TryGetProperty("type", out JsonElement typeElement)
                    ? typeElement.GetString() ?? "unknown"
                    : "unknown";
                if (type == "title") { continue; }

                rows.Add((property.Name, FormatPropertyValue(property.Value, type)));
            }

            if (rows.Count == 0) { return string.Empty; }

            StringBuilder markdown = new();
            markdown.AppendLine("| プロパティ | 値 |");
            markdown.AppendLine("|---|---|");
            foreach ((string name, string value) in rows.OrderBy(row => row.Name, StringComparer.OrdinalIgnoreCase))
            {
                markdown.Append("| ");
                markdown.Append(EscapeTableCell(name));
                markdown.Append(" | ");
                markdown.Append(EscapeTableCell(value));
                markdown.AppendLine(" |");
            }

            return markdown.ToString().TrimEnd();
        }

        /// <summary>
        ///     単一プロパティを表示文字列へ変換する。
        /// </summary>
        /// <param name="property">プロパティJSON。</param>
        /// <param name="type">プロパティ型。</param>
        /// <returns>表示文字列。</returns>
        private static string FormatPropertyValue(JsonElement property, string type)
        {
            if (!property.TryGetProperty(type, out JsonElement value)) { return string.Empty; }

            return type switch
            {
                "title" or "rich_text" => ReadRichText(value),
                "number" => ReadScalar(value),
                "checkbox" => value.ValueKind == JsonValueKind.True ? "true" : "false",
                "select" or "status" => ReadNamedObject(value),
                "multi_select" => JoinNamedObjects(value),
                "date" => FormatDate(value),
                "people" => JoinPeople(value),
                "created_by" or "last_edited_by" => ReadPerson(value),
                "files" => FormatFiles(value),
                "relation" => JoinRelations(value),
                "formula" => FormatTypedValue(value),
                "rollup" => FormatTypedValue(value),
                "unique_id" => FormatUniqueId(value),
                "verification" => ReadNamedObject(value),
                _ => ReadScalar(value)
            };
        }

        /// <summary>
        ///     typeフィールドを持つNotion値を再帰的に表示文字列へ変換する。
        /// </summary>
        /// <param name="value">型付き値。</param>
        /// <returns>表示文字列。</returns>
        private static string FormatTypedValue(JsonElement value)
        {
            if (!value.TryGetProperty("type", out JsonElement typeElement)) { return ReadScalar(value); }

            string type = typeElement.GetString() ?? string.Empty;
            if (!value.TryGetProperty(type, out JsonElement typedValue)) { return string.Empty; }

            return type switch
            {
                "string" => typedValue.GetString() ?? string.Empty,
                "number" or "boolean" or "date" => ReadScalar(typedValue),
                "array" when typedValue.ValueKind == JsonValueKind.Array => string.Join(", ", typedValue.EnumerateArray().Select(FormatTypedValue)),
                _ => ReadScalar(typedValue)
            };
        }

        /// <summary>
        ///     日付プロパティを期間表記へ変換する。
        /// </summary>
        /// <param name="value">日付オブジェクト。</param>
        /// <returns>日付または期間。</returns>
        private static string FormatDate(JsonElement value)
        {
            if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined) { return string.Empty; }

            string start = value.TryGetProperty("start", out JsonElement startElement)
                ? startElement.GetString() ?? string.Empty
                : string.Empty;
            string end = value.TryGetProperty("end", out JsonElement endElement) && endElement.ValueKind == JsonValueKind.String
                ? endElement.GetString() ?? string.Empty
                : string.Empty;
            return string.IsNullOrWhiteSpace(end) ? start : $"{start} ～ {end}";
        }

        /// <summary>
        ///     nameフィールドを持つオブジェクトから名称を取得する。
        /// </summary>
        /// <param name="value">Notionオブジェクト。</param>
        /// <returns>名称。</returns>
        private static string ReadNamedObject(JsonElement value)
        {
            if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined) { return string.Empty; }
            return value.TryGetProperty("name", out JsonElement nameElement)
                ? nameElement.GetString() ?? string.Empty
                : ReadScalar(value);
        }

        /// <summary>
        ///     nameフィールドを持つオブジェクト配列を連結する。
        /// </summary>
        /// <param name="value">Notionオブジェクト配列。</param>
        /// <returns>名称一覧。</returns>
        private static string JoinNamedObjects(JsonElement value)
        {
            if (value.ValueKind != JsonValueKind.Array) { return string.Empty; }
            return string.Join(", ", value.EnumerateArray().Select(ReadNamedObject));
        }

        /// <summary>
        ///     ユーザー配列を表示名へ変換する。
        /// </summary>
        /// <param name="value">ユーザー配列。</param>
        /// <returns>ユーザー名一覧。</returns>
        private static string JoinPeople(JsonElement value)
        {
            if (value.ValueKind != JsonValueKind.Array) { return string.Empty; }
            return string.Join(", ", value.EnumerateArray().Select(ReadPerson));
        }

        /// <summary>
        ///     Notionユーザーを表示名へ変換する。
        /// </summary>
        /// <param name="value">ユーザーオブジェクト。</param>
        /// <returns>表示名またはID。</returns>
        private static string ReadPerson(JsonElement value)
        {
            if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined) { return string.Empty; }
            if (value.TryGetProperty("name", out JsonElement nameElement) && !string.IsNullOrWhiteSpace(nameElement.GetString()))
            {
                return nameElement.GetString() ?? string.Empty;
            }

            return value.TryGetProperty("id", out JsonElement idElement)
                ? idElement.GetString() ?? string.Empty
                : string.Empty;
        }

        /// <summary>
        ///     ファイルプロパティをEnhanced Markdownのfileタグへ変換する。
        /// </summary>
        /// <param name="value">ファイル配列。</param>
        /// <returns>ファイル一覧。</returns>
        private static string FormatFiles(JsonElement value)
        {
            if (value.ValueKind != JsonValueKind.Array) { return string.Empty; }

            List<string> files = new();
            foreach (JsonElement file in value.EnumerateArray())
            {
                string name = file.TryGetProperty("name", out JsonElement nameElement)
                    ? nameElement.GetString() ?? "file"
                    : "file";
                string? type = file.TryGetProperty("type", out JsonElement typeElement)
                    ? typeElement.GetString()
                    : null;
                if (type != null && file.TryGetProperty(type, out JsonElement source) &&
                    source.TryGetProperty("url", out JsonElement urlElement) &&
                    !string.IsNullOrWhiteSpace(urlElement.GetString()))
                {
                    files.Add($"<file src=\"{urlElement.GetString()}\">{name}</file>");
                }
                else
                {
                    files.Add(name);
                }
            }

            return string.Join("<br>", files);
        }

        /// <summary>
        ///     RelationプロパティをページID一覧へ変換する。
        /// </summary>
        /// <param name="value">Relation配列。</param>
        /// <returns>ページID一覧。</returns>
        private static string JoinRelations(JsonElement value)
        {
            if (value.ValueKind != JsonValueKind.Array) { return string.Empty; }
            return string.Join(", ", value.EnumerateArray().Select(item =>
                item.TryGetProperty("id", out JsonElement idElement)
                    ? idElement.GetString() ?? string.Empty
                    : string.Empty));
        }

        /// <summary>
        ///     unique_idプロパティを接頭辞付き文字列へ変換する。
        /// </summary>
        /// <param name="value">unique_idオブジェクト。</param>
        /// <returns>識別子。</returns>
        private static string FormatUniqueId(JsonElement value)
        {
            string prefix = value.TryGetProperty("prefix", out JsonElement prefixElement) &&
                            prefixElement.ValueKind == JsonValueKind.String
                ? prefixElement.GetString() ?? string.Empty
                : string.Empty;
            string number = value.TryGetProperty("number", out JsonElement numberElement)
                ? ReadScalar(numberElement)
                : string.Empty;
            return $"{prefix}{number}";
        }

        /// <summary>
        ///     JSON値を簡潔な文字列へ変換する。
        /// </summary>
        /// <param name="value">JSON値。</param>
        /// <returns>表示文字列。</returns>
        private static string ReadScalar(JsonElement value)
        {
            return value.ValueKind switch
            {
                JsonValueKind.Null or JsonValueKind.Undefined => string.Empty,
                JsonValueKind.String => value.GetString() ?? string.Empty,
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => value.GetRawText()
            };
        }

        /// <summary>
        ///     Markdownテーブル内で特殊扱いされる文字をエスケープする。
        /// </summary>
        /// <param name="value">セル内容。</param>
        /// <returns>エスケープ済み文字列。</returns>
        private static string EscapeTableCell(string value)
        {
            return value.Replace("|", "\\|", StringComparison.Ordinal)
                .Replace("\r\n", "<br>", StringComparison.Ordinal)
                .Replace("\n", "<br>", StringComparison.Ordinal)
                .Replace("\r", "<br>", StringComparison.Ordinal);
        }
    }
}
