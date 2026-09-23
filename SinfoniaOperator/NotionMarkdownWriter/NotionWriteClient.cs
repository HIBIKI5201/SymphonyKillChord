using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using SinfoniaStudio.NotionMarkdownExporter;

namespace SinfoniaStudio.NotionMarkdownWriter
{
    /// <summary>
    ///     Notionのオブジェクト種別。祖先チェーンを辿る際の問い合わせ先を決める。
    /// </summary>
    internal enum NotionObjectKind
    {
        Page,
        Block,
        DataSource,
        Database
    }

    /// <summary>
    ///     Notionオブジェクトの親への参照を保持するクラス。
    /// </summary>
    internal sealed class NotionParentReference
    {
        /// <summary>
        ///     親参照を生成する。
        /// </summary>
        /// <param name="type">Notionが返す親の種別文字列。</param>
        /// <param name="id">親のID。ワークスペース直下の場合は空。</param>
        internal NotionParentReference(string type, string id)
        {
            Type = type;
            Id = id;
        }

        /// <summary> 親の種別。page_id、block_id、data_source_id、database_id、workspaceなど。 </summary>
        internal string Type { get; }

        /// <summary> 親のID。 </summary>
        internal string Id { get; }
    }

    /// <summary>
    ///     編集対象ページの識別情報を保持するクラス。
    /// </summary>
    internal sealed class NotionPageInfo
    {
        /// <summary>
        ///     ページ情報を生成する。
        /// </summary>
        /// <param name="id">ページID。</param>
        /// <param name="url">ページURL。</param>
        /// <param name="title">ページタイトル。</param>
        /// <param name="lastEditedTime">最終更新日時。競合検出に使用する。</param>
        /// <param name="parent">親への参照。</param>
        internal NotionPageInfo(
            string id,
            string url,
            string title,
            string lastEditedTime,
            NotionParentReference parent)
        {
            Id = id;
            Url = url;
            Title = title;
            LastEditedTime = lastEditedTime;
            Parent = parent;
        }

        internal string Id { get; }
        internal string Url { get; }
        internal string Title { get; }
        internal string LastEditedTime { get; }
        internal NotionParentReference Parent { get; }
    }

    /// <summary>
    ///     データベースのスキーマと識別情報を保持するクラス。
    /// </summary>
    internal sealed class NotionDatabaseInfo
    {
        /// <summary>
        ///     データベース情報を生成する。
        /// </summary>
        /// <param name="id">データベースID。</param>
        /// <param name="url">データベースURL。</param>
        /// <param name="title">データベース名。</param>
        /// <param name="parent">親への参照。</param>
        /// <param name="propertyTypes">プロパティ名とNotion上の型。</param>
        internal NotionDatabaseInfo(
            string id,
            string dataSourceId,
            string url,
            string title,
            NotionParentReference parent,
            IReadOnlyDictionary<string, string> propertyTypes)
        {
            Id = id;
            DataSourceId = dataSourceId;
            Url = url;
            Title = title;
            Parent = parent;
            PropertyTypes = propertyTypes;
        }

        internal string Id { get; }

        /// <summary> スキーマを持つデータソースのID。行の作成先にはこちらを指定する。 </summary>
        internal string DataSourceId { get; }
        internal string Url { get; }
        internal string Title { get; }
        internal NotionParentReference Parent { get; }

        /// <summary> プロパティ名とNotion上の型。 </summary>
        internal IReadOnlyDictionary<string, string> PropertyTypes { get; }

        /// <summary>
        ///     タイトルにあたるプロパティ名を取得する。
        /// </summary>
        /// <returns>タイトルプロパティ名。見つからない場合はnull。</returns>
        internal string? FindTitlePropertyName()
        {
            foreach (KeyValuePair<string, string> property in PropertyTypes)
            {
                if (property.Value == "title") { return property.Key; }
            }

            return null;
        }
    }

    /// <summary>
    ///     Markdown Content APIを用いてNotionページを取得・作成・更新するクライアント。
    /// </summary>
    internal sealed class NotionWriteClient : IDisposable
    {
        private const string API_BASE_URL = "https://api.notion.com/v1";
        private const string NOTION_API_VERSION = "2026-03-11";
        private const int MAX_RETRY_COUNT = 4;

        /// <summary> Notion APIの実効レート制限（目安: 秒間3リクエスト）に対して余裕を持たせた送信ペース。 </summary>
        private const double REQUESTS_PER_SECOND = 2.5;

        /// <summary> 起動直後などに許容する瞬間的なバースト送信数。 </summary>
        private const double REQUEST_BURST_CAPACITY = 3;

        private static readonly JsonSerializerOptions _requestJsonOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private readonly HttpClient _httpClient;
        private readonly RequestRateLimiter _rateLimiter = new(REQUESTS_PER_SECOND, REQUEST_BURST_CAPACITY);
        private bool _isDisposed;

        /// <summary>
        ///     書き込み用のNotion APIクライアントを生成する。
        /// </summary>
        /// <param name="notionToken">Notion内部インテグレーションのトークン。</param>
        internal NotionWriteClient(string notionToken)
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", notionToken);
            _httpClient.DefaultRequestHeaders.Add("Notion-Version", NOTION_API_VERSION);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SinfoniaOperator-NotionMarkdownWriter/1.0");
        }

        /// <summary>
        ///     ページのメタデータを取得する。
        /// </summary>
        /// <param name="pageId">ページID。</param>
        /// <returns>ページ情報。</returns>
        internal async Task<NotionPageInfo> GetPageAsync(string pageId)
        {
            string responseBody = await SendAsync(
                HttpMethod.Get,
                $"{API_BASE_URL}/pages/{Uri.EscapeDataString(pageId)}",
                null,
                true);
            using JsonDocument document = JsonDocument.Parse(responseBody);
            return ParsePageInfo(document.RootElement);
        }

        /// <summary>
        ///     ページ本文を加工前のEnhanced Markdownとして取得する。
        ///     エクスポート済みMarkdownはリンク変換や装飾除去を経ているため、
        ///     編集の基準にはこちらの原文を使う。
        /// </summary>
        /// <param name="pageId">ページID。</param>
        /// <returns>Markdown原文。</returns>
        internal async Task<string> GetMarkdownAsync(string pageId)
        {
            string responseBody = await SendAsync(
                HttpMethod.Get,
                $"{API_BASE_URL}/pages/{Uri.EscapeDataString(pageId)}/markdown",
                null,
                true);
            using JsonDocument document = JsonDocument.Parse(responseBody);
            JsonElement root = document.RootElement;

            // 分割取得されたページは原文が欠けるため、そのまま差分を作ると本文を破壊しかねない。
            if (root.TryGetProperty("truncated", out JsonElement truncated) &&
                truncated.ValueKind == JsonValueKind.True)
            {
                throw new WriterException(
                    "ページが大きすぎてAPIが本文を分割しました。このツールでは編集できません。Notion上で直接編集してください。");
            }

            return root.TryGetProperty("markdown", out JsonElement markdown)
                ? markdown.GetString() ?? string.Empty
                : string.Empty;
        }

        /// <summary>
        ///     指定オブジェクトの親を取得する。
        /// </summary>
        /// <param name="kind">オブジェクト種別。</param>
        /// <param name="id">オブジェクトID。</param>
        /// <returns>親への参照。</returns>
        internal async Task<NotionParentReference> GetParentAsync(NotionObjectKind kind, string id)
        {
            string segment = kind switch
            {
                NotionObjectKind.Page => "pages",
                NotionObjectKind.Block => "blocks",
                NotionObjectKind.DataSource => "data_sources",
                NotionObjectKind.Database => "databases",
                _ => throw new WriterException($"未対応のオブジェクト種別です: {kind}")
            };
            string responseBody = await SendAsync(
                HttpMethod.Get,
                $"{API_BASE_URL}/{segment}/{Uri.EscapeDataString(id)}",
                null,
                true);
            using JsonDocument document = JsonDocument.Parse(responseBody);
            return ParseParent(document.RootElement);
        }

        /// <summary>
        ///     データベースのメタデータとスキーマを取得する。
        /// </summary>
        /// <param name="databaseId">データベースID。</param>
        /// <returns>データベース情報。</returns>
        internal async Task<NotionDatabaseInfo> GetDatabaseAsync(string databaseId)
        {
            string responseBody = await SendAsync(
                HttpMethod.Get,
                $"{API_BASE_URL}/databases/{Uri.EscapeDataString(databaseId)}",
                null,
                true);
            using JsonDocument document = JsonDocument.Parse(responseBody);
            JsonElement root = document.RootElement;

            string id = root.TryGetProperty("id", out JsonElement idElement) ? idElement.GetString() ?? string.Empty : string.Empty;
            string url = root.TryGetProperty("url", out JsonElement urlElement) ? urlElement.GetString() ?? string.Empty : string.Empty;

            // Notion-Version 2026-03-11 では、プロパティのスキーマはデータベースではなくデータソースが持つ。
            string dataSourceId = ParseFirstDataSourceId(root);
            if (string.IsNullOrEmpty(dataSourceId))
            {
                throw new WriterException($"データベース {id} にデータソースがありません。");
            }

            string schemaBody = await SendAsync(
                HttpMethod.Get,
                $"{API_BASE_URL}/data_sources/{Uri.EscapeDataString(dataSourceId)}",
                null,
                true);
            using JsonDocument schemaDocument = JsonDocument.Parse(schemaBody);
            Dictionary<string, string> propertyTypes = new();
            if (schemaDocument.RootElement.TryGetProperty("properties", out JsonElement properties) &&
                properties.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in properties.EnumerateObject())
                {
                    string type = property.Value.ValueKind == JsonValueKind.Object &&
                                  property.Value.TryGetProperty("type", out JsonElement typeElement)
                        ? typeElement.GetString() ?? string.Empty
                        : string.Empty;
                    propertyTypes[property.Name] = type;
                }
            }

            return new NotionDatabaseInfo(id, dataSourceId, url, ParseRichTextTitle(root), ParseParent(root), propertyTypes);
        }

        /// <summary>
        ///     データベースJSONから最初のデータソースIDを取り出す。
        /// </summary>
        /// <param name="root">データベースオブジェクトのJSON。</param>
        /// <returns>データソースID。存在しない場合は空文字。</returns>
        private static string ParseFirstDataSourceId(JsonElement root)
        {
            if (!root.TryGetProperty("data_sources", out JsonElement dataSources) ||
                dataSources.ValueKind != JsonValueKind.Array)
            {
                return string.Empty;
            }

            foreach (JsonElement dataSource in dataSources.EnumerateArray())
            {
                if (dataSource.TryGetProperty("id", out JsonElement idElement))
                {
                    return idElement.GetString() ?? string.Empty;
                }
            }

            return string.Empty;
        }

        /// <summary>
        ///     Markdownを本文として新しい子ページを作成する。
        /// </summary>
        /// <param name="parent">親の指定。page_idまたはdatabase_idを1件だけ含める。</param>
        /// <param name="markdown">ページ本文のMarkdown。</param>
        /// <param name="properties">設定するプロパティ。ページ直下へ作成する場合はnull。</param>
        /// <returns>作成されたページ情報。</returns>
        internal async Task<NotionPageInfo> CreatePageAsync(
            IReadOnlyDictionary<string, string> parent,
            string markdown,
            IReadOnlyDictionary<string, object>? properties)
        {
            Dictionary<string, object> requestBody = new()
            {
                ["parent"] = parent,
                ["markdown"] = markdown
            };
            if (properties != null && properties.Count > 0) { requestBody["properties"] = properties; }

            string json = JsonSerializer.Serialize(requestBody, _requestJsonOptions);

            // POSTの再送はページの二重作成につながるため、サーバーエラーでは再試行しない。
            string responseBody = await SendAsync(HttpMethod.Post, $"{API_BASE_URL}/pages", json, false);
            using JsonDocument document = JsonDocument.Parse(responseBody);
            return ParsePageInfo(document.RootElement);
        }

        /// <summary>
        ///     既存ページのタイトルだけを更新する。本文には触れない。
        /// </summary>
        /// <param name="pageId">ページID。</param>
        /// <param name="title">新しいページ名。</param>
        internal async Task UpdatePageTitleAsync(string pageId, string title)
        {
            Dictionary<string, object> requestBody = new()
            {
                ["properties"] = new Dictionary<string, object>
                {
                    ["title"] = new Dictionary<string, object>
                    {
                        ["title"] = new List<Dictionary<string, object>>
                        {
                            new()
                            {
                                ["type"] = "text",
                                ["text"] = new Dictionary<string, string> { ["content"] = title }
                            }
                        }
                    }
                }
            };

            string json = JsonSerializer.Serialize(requestBody, _requestJsonOptions);
            await SendAsync(
                HttpMethod.Patch,
                $"{API_BASE_URL}/pages/{Uri.EscapeDataString(pageId)}",
                json,
                true);
        }

        /// <summary>
        ///     既存ページの本文を部分置換で更新する。
        ///     全文置換（replace_content）と子ページ削除（allow_deleting_content）は意図的に実装しない。
        /// </summary>
        /// <param name="pageId">ページID。</param>
        /// <param name="updates">適用する置換内容。</param>
        internal async Task UpdateMarkdownAsync(string pageId, IReadOnlyList<ContentUpdate> updates)
        {
            List<Dictionary<string, string>> contentUpdates = new();
            foreach (ContentUpdate update in updates)
            {
                contentUpdates.Add(new Dictionary<string, string>
                {
                    ["old_str"] = update.OldString,
                    ["new_str"] = update.NewString
                });
            }

            Dictionary<string, object> requestBody = new()
            {
                ["type"] = "update_content",
                ["update_content"] = new Dictionary<string, object>
                {
                    ["content_updates"] = contentUpdates
                }
            };
            string json = JsonSerializer.Serialize(requestBody, _requestJsonOptions);
            await SendAsync(
                HttpMethod.Patch,
                $"{API_BASE_URL}/pages/{Uri.EscapeDataString(pageId)}/markdown",
                json,
                false);
        }

        /// <summary>
        ///     HTTPクライアントを解放する。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) { return; }

            _httpClient.Dispose();
            _isDisposed = true;
        }

        /// <summary>
        ///     ページJSONからメタデータを取り出す。
        /// </summary>
        /// <param name="root">ページオブジェクトのJSON。</param>
        /// <returns>ページ情報。</returns>
        private static NotionPageInfo ParsePageInfo(JsonElement root)
        {
            string id = root.TryGetProperty("id", out JsonElement idElement) ? idElement.GetString() ?? string.Empty : string.Empty;
            string url = root.TryGetProperty("url", out JsonElement urlElement) ? urlElement.GetString() ?? string.Empty : string.Empty;
            string lastEditedTime = root.TryGetProperty("last_edited_time", out JsonElement editedElement)
                ? editedElement.GetString() ?? string.Empty
                : string.Empty;
            return new NotionPageInfo(id, url, ParseTitle(root), lastEditedTime, ParseParent(root));
        }

        /// <summary>
        ///     ページJSONのプロパティからタイトルを組み立てる。
        /// </summary>
        /// <param name="root">ページオブジェクトのJSON。</param>
        /// <returns>タイトル。取得できない場合は空文字。</returns>
        private static string ParseTitle(JsonElement root)
        {
            if (!root.TryGetProperty("properties", out JsonElement properties) ||
                properties.ValueKind != JsonValueKind.Object)
            {
                return string.Empty;
            }

            foreach (JsonProperty property in properties.EnumerateObject())
            {
                if (property.Value.ValueKind != JsonValueKind.Object) { continue; }
                if (!property.Value.TryGetProperty("type", out JsonElement type) ||
                    type.GetString() != "title")
                {
                    continue;
                }

                if (!property.Value.TryGetProperty("title", out JsonElement titleArray) ||
                    titleArray.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                StringBuilder builder = new();
                foreach (JsonElement item in titleArray.EnumerateArray())
                {
                    if (item.TryGetProperty("plain_text", out JsonElement plainText))
                    {
                        builder.Append(plainText.GetString());
                    }
                }

                return builder.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        ///     データベースJSONのtitle配列から名称を組み立てる。
        /// </summary>
        /// <param name="root">データベースオブジェクトのJSON。</param>
        /// <returns>名称。取得できない場合は空文字。</returns>
        private static string ParseRichTextTitle(JsonElement root)
        {
            if (!root.TryGetProperty("title", out JsonElement titleArray) ||
                titleArray.ValueKind != JsonValueKind.Array)
            {
                return string.Empty;
            }

            StringBuilder builder = new();
            foreach (JsonElement item in titleArray.EnumerateArray())
            {
                if (item.TryGetProperty("plain_text", out JsonElement plainText))
                {
                    builder.Append(plainText.GetString());
                }
            }

            return builder.ToString();
        }

        /// <summary>
        ///     オブジェクトJSONから親への参照を取り出す。
        /// </summary>
        /// <param name="root">オブジェクトのJSON。</param>
        /// <returns>親への参照。</returns>
        private static NotionParentReference ParseParent(JsonElement root)
        {
            if (!root.TryGetProperty("parent", out JsonElement parent) || parent.ValueKind != JsonValueKind.Object)
            {
                return new NotionParentReference("unknown", string.Empty);
            }

            string type = parent.TryGetProperty("type", out JsonElement typeElement)
                ? typeElement.GetString() ?? "unknown"
                : "unknown";
            string id = parent.TryGetProperty(type, out JsonElement idElement) && idElement.ValueKind == JsonValueKind.String
                ? idElement.GetString() ?? string.Empty
                : string.Empty;
            return new NotionParentReference(type, id);
        }

        /// <summary>
        ///     Notion APIへリクエストを送信し、必要に応じて再試行する。
        /// </summary>
        /// <param name="method">HTTPメソッド。</param>
        /// <param name="url">リクエストURL。</param>
        /// <param name="json">任意のJSON本文。</param>
        /// <param name="retriesServerErrors">サーバーエラーで再試行してよいかどうか。書き込みでは二重適用を避けるためfalseにする。</param>
        /// <returns>レスポンス本文。</returns>
        private async Task<string> SendAsync(HttpMethod method, string url, string? json, bool retriesServerErrors)
        {
            for (int attempt = 1; attempt <= MAX_RETRY_COUNT; attempt++)
            {
                using HttpRequestMessage request = new(method, url);
                if (json != null)
                {
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                await _rateLimiter.WaitAsync();
                using HttpResponseMessage response = await _httpClient.SendAsync(request);
                string responseBody = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode) { return responseBody; }

                bool canRetry = response.StatusCode == HttpStatusCode.TooManyRequests ||
                                (retriesServerErrors && (int)response.StatusCode >= 500);
                if (canRetry && attempt < MAX_RETRY_COUNT)
                {
                    TimeSpan waitTime = GetRetryDelay(response, attempt);
                    Console.Error.WriteLine($"  Notion APIが混雑しています。{waitTime.TotalSeconds:0}秒後に再試行します。");
                    await Task.Delay(waitTime);
                    continue;
                }

                string error = responseBody.Length > 800 ? responseBody[..800] : responseBody;
                throw new NotionApiException(
                    response.StatusCode,
                    $"Notion APIが {(int)response.StatusCode} {response.ReasonPhrase} を返しました。{error}");
            }

            throw new WriterException("Notion APIへの再試行回数を超えました。");
        }

        /// <summary>
        ///     Retry-Afterヘッダーまたは指数バックオフから再試行までの待機時間を決定する。
        /// </summary>
        /// <param name="response">HTTPレスポンス。</param>
        /// <param name="attempt">試行回数。</param>
        /// <returns>待機時間。</returns>
        private static TimeSpan GetRetryDelay(HttpResponseMessage response, int attempt)
        {
            RetryConditionHeaderValue? retryAfter = response.Headers.RetryAfter;
            if (retryAfter?.Delta != null) { return retryAfter.Delta.Value; }
            if (retryAfter?.Date != null)
            {
                TimeSpan difference = retryAfter.Date.Value - DateTimeOffset.UtcNow;
                if (difference > TimeSpan.Zero) { return difference; }
            }

            return TimeSpan.FromSeconds(Math.Pow(2, attempt));
        }
    }
}
