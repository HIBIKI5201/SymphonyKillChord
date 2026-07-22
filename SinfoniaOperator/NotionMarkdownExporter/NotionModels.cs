using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SinfoniaStudio.NotionMarkdownExporter
{
    /// <summary>
    ///     Notionページのメタデータを保持するクラス。
    /// </summary>
    internal sealed class PageMetadata
    {
        /// <summary>
        ///     Notionページのメタデータを生成する。
        /// </summary>
        /// <param name="id">ページID。</param>
        /// <param name="title">ページタイトル。</param>
        /// <param name="url">Notion上のURL。</param>
        /// <param name="lastEditedTime">最終更新日時。</param>
        /// <param name="properties">ページプロパティ。</param>
        internal PageMetadata(
            string id,
            string title,
            string url,
            DateTimeOffset? lastEditedTime,
            JsonElement properties)
        {
            Id = id;
            Title = title;
            Url = url;
            LastEditedTime = lastEditedTime;
            Properties = properties;
        }

        internal string Id { get; }
        internal string Title { get; }
        internal string Url { get; }
        internal DateTimeOffset? LastEditedTime { get; }
        internal JsonElement Properties { get; }

        /// <summary>
        ///     Notion APIのページJSONからメタデータを生成する。
        /// </summary>
        /// <param name="element">ページJSON。</param>
        /// <returns>ページメタデータ。</returns>
        internal static PageMetadata FromJson(JsonElement element)
        {
            string id = element.GetProperty("id").GetString() ?? throw new InvalidOperationException("ページIDがありません。");
            string url = element.TryGetProperty("url", out JsonElement urlElement)
                ? urlElement.GetString() ?? string.Empty
                : string.Empty;
            DateTimeOffset? lastEditedTime = null;
            if (element.TryGetProperty("last_edited_time", out JsonElement timeElement) &&
                DateTimeOffset.TryParse(timeElement.GetString(), out DateTimeOffset parsedTime))
            {
                lastEditedTime = parsedTime;
            }

            JsonElement properties = element.TryGetProperty("properties", out JsonElement propertyElement)
                ? propertyElement.Clone()
                : JsonDocument.Parse("{}").RootElement.Clone();
            string title = PropertyFormatter.FindTitle(properties);
            if (string.IsNullOrWhiteSpace(title)) { title = "名称未設定"; }

            return new PageMetadata(id, title, url, lastEditedTime, properties);
        }
    }

    /// <summary>
    ///     Notionページから取得したMarkdownと警告を保持するクラス。
    /// </summary>
    internal sealed class MarkdownPayload
    {
        /// <summary>
        ///     Markdown取得結果を生成する。
        /// </summary>
        /// <param name="markdown">取得したMarkdown。</param>
        /// <param name="warnings">取得時の警告。</param>
        internal MarkdownPayload(string markdown, IReadOnlyList<string> warnings)
        {
            Markdown = markdown;
            Warnings = warnings;
        }

        internal string Markdown { get; }
        internal IReadOnlyList<string> Warnings { get; }
    }

    /// <summary>
    ///     Notionデータベースのメタデータを保持するクラス。
    /// </summary>
    internal sealed class DatabaseMetadata
    {
        /// <summary>
        ///     データベースのメタデータを生成する。
        /// </summary>
        /// <param name="id">データベースID。</param>
        /// <param name="title">データベース名。</param>
        /// <param name="url">Notion上のURL。</param>
        /// <param name="dataSources">配下のデータソース。</param>
        internal DatabaseMetadata(
            string id,
            string title,
            string url,
            IReadOnlyList<DataSourceReference> dataSources)
        {
            Id = id;
            Title = title;
            Url = url;
            DataSources = dataSources;
        }

        internal string Id { get; }
        internal string Title { get; }
        internal string Url { get; }
        internal IReadOnlyList<DataSourceReference> DataSources { get; }

        /// <summary>
        ///     Notion APIのデータベースJSONからメタデータを生成する。
        /// </summary>
        /// <param name="element">データベースJSON。</param>
        /// <returns>データベースメタデータ。</returns>
        internal static DatabaseMetadata FromJson(JsonElement element)
        {
            string id = element.GetProperty("id").GetString() ?? throw new InvalidOperationException("データベースIDがありません。");
            string title = element.TryGetProperty("title", out JsonElement titleElement)
                ? PropertyFormatter.ReadRichText(titleElement)
                : string.Empty;
            if (string.IsNullOrWhiteSpace(title)) { title = "名称未設定データベース"; }

            string url = element.TryGetProperty("url", out JsonElement urlElement)
                ? urlElement.GetString() ?? string.Empty
                : string.Empty;
            List<DataSourceReference> dataSources = new();
            if (element.TryGetProperty("data_sources", out JsonElement sourcesElement) &&
                sourcesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement source in sourcesElement.EnumerateArray())
                {
                    string? sourceId = source.TryGetProperty("id", out JsonElement sourceIdElement)
                        ? sourceIdElement.GetString()
                        : null;
                    if (string.IsNullOrWhiteSpace(sourceId)) { continue; }

                    string sourceName = source.TryGetProperty("name", out JsonElement nameElement)
                        ? nameElement.GetString() ?? title
                        : title;
                    dataSources.Add(new DataSourceReference(sourceId, sourceName));
                }
            }

            return new DatabaseMetadata(id, title, url, dataSources);
        }
    }

    /// <summary>
    ///     データソースへの参照を保持するクラス。
    /// </summary>
    internal sealed class DataSourceReference
    {
        /// <summary>
        ///     データソース参照を生成する。
        /// </summary>
        /// <param name="id">データソースID。</param>
        /// <param name="name">データソース名。</param>
        internal DataSourceReference(string id, string name)
        {
            Id = id;
            Name = name;
        }

        internal string Id { get; }
        internal string Name { get; }
    }

    /// <summary>
    ///     データソースのスキーマを保持するクラス。
    /// </summary>
    internal sealed class DataSourceSchema
    {
        /// <summary>
        ///     データソースのスキーマを生成する。
        /// </summary>
        /// <param name="id">データソースID。</param>
        /// <param name="properties">プロパティ一覧。</param>
        internal DataSourceSchema(string id, IReadOnlyList<SchemaProperty> properties)
        {
            Id = id;
            Properties = properties;
        }

        internal string Id { get; }
        internal IReadOnlyList<SchemaProperty> Properties { get; }

        /// <summary>
        ///     Notion APIのデータソースJSONからスキーマを生成する。
        /// </summary>
        /// <param name="element">データソースJSON。</param>
        /// <returns>データソーススキーマ。</returns>
        internal static DataSourceSchema FromJson(JsonElement element)
        {
            string id = element.GetProperty("id").GetString() ?? throw new InvalidOperationException("データソースIDがありません。");
            List<SchemaProperty> properties = new();
            if (element.TryGetProperty("properties", out JsonElement propertiesElement) &&
                propertiesElement.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in propertiesElement.EnumerateObject())
                {
                    string type = property.Value.TryGetProperty("type", out JsonElement typeElement)
                        ? typeElement.GetString() ?? "unknown"
                        : "unknown";
                    properties.Add(new SchemaProperty(property.Name, type));
                }
            }

            return new DataSourceSchema(id, properties);
        }
    }

    /// <summary>
    ///     データソースのプロパティ定義を保持するクラス。
    /// </summary>
    internal sealed class SchemaProperty
    {
        /// <summary>
        ///     プロパティ定義を生成する。
        /// </summary>
        /// <param name="name">プロパティ名。</param>
        /// <param name="type">プロパティ型。</param>
        internal SchemaProperty(string name, string type)
        {
            Name = name;
            Type = type;
        }

        internal string Name { get; }
        internal string Type { get; }
    }

    /// <summary>
    ///     Markdown内の子ページまたはデータベース参照を保持するクラス。
    /// </summary>
    internal sealed class MarkdownReference
    {
        /// <summary>
        ///     Markdown参照を生成する。
        /// </summary>
        /// <param name="id">参照先ID。</param>
        /// <param name="title">参照先タイトル。</param>
        /// <param name="url">参照先URL。</param>
        internal MarkdownReference(string id, string title, string url)
        {
            Id = id;
            Title = title;
            Url = url;
        }

        internal string Id { get; }
        internal string Title { get; }
        internal string Url { get; }
    }
}
