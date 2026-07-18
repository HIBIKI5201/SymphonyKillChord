using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SinfoniaStudio.NotionMarkdownExporter
{
    /// <summary>
    ///     Enhanced Markdown APIを含むNotion APIへのアクセスを提供するクラス。
    /// </summary>
    internal sealed class NotionApiClient : IDisposable
    {
        private const string API_BASE_URL = "https://api.notion.com/v1";
        private const string NOTION_API_VERSION = "2026-03-11";
        private const int MAX_RETRY_COUNT = 4;
        private const int DATA_SOURCE_PAGE_SIZE = 25;

        private readonly HttpClient _httpClient;
        private bool _isDisposed;

        /// <summary>
        ///     Notion APIクライアントを生成する。
        /// </summary>
        /// <param name="notionToken">Notion内部インテグレーションのトークン。</param>
        internal NotionApiClient(string notionToken)
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", notionToken);
            _httpClient.DefaultRequestHeaders.Add("Notion-Version", NOTION_API_VERSION);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SinfoniaOperator-NotionMarkdownExporter/1.0");
        }

        /// <summary>
        ///     指定ページのメタデータを取得する。
        /// </summary>
        /// <param name="pageId">ページID。</param>
        /// <returns>ページメタデータ。</returns>
        internal async Task<PageMetadata> GetPageAsync(string pageId)
        {
            using JsonDocument document = await GetJsonAsync($"{API_BASE_URL}/pages/{Uri.EscapeDataString(pageId)}");
            return PageMetadata.FromJson(document.RootElement);
        }

        /// <summary>
        ///     指定ページをEnhanced Markdownとして取得し、分割された巨大ページも可能な範囲で補完する。
        /// </summary>
        /// <param name="pageId">ページID。</param>
        /// <returns>Markdown取得結果。</returns>
        internal async Task<MarkdownPayload> GetCompleteMarkdownAsync(string pageId)
        {
            MarkdownFragment root = await GetMarkdownFragmentAsync(pageId);
            StringBuilder markdown = new(root.Markdown);
            List<string> warnings = new();
            Queue<string> pendingIds = new(root.UnknownBlockIds);
            HashSet<string> fetchedIds = new(StringComparer.OrdinalIgnoreCase);

            if (root.IsTruncated)
            {
                warnings.Add($"ページ {pageId} はAPI上限により分割取得されました。");
            }

            while (pendingIds.Count > 0)
            {
                string blockId = pendingIds.Dequeue();
                if (!fetchedIds.Add(blockId)) { continue; }

                try
                {
                    MarkdownFragment fragment = await GetMarkdownFragmentAsync(blockId);
                    markdown.AppendLine();
                    markdown.AppendLine();
                    markdown.AppendLine($"<!-- truncated-block: {blockId} -->");
                    markdown.Append(fragment.Markdown);
                    foreach (string childId in fragment.UnknownBlockIds)
                    {
                        pendingIds.Enqueue(childId);
                    }
                }
                catch (NotionApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    warnings.Add($"ブロック {blockId} は権限不足のため取得できませんでした。");
                }
            }

            return new MarkdownPayload(markdown.ToString(), warnings);
        }

        /// <summary>
        ///     指定データベースのメタデータを取得する。
        /// </summary>
        /// <param name="databaseId">データベースID。</param>
        /// <returns>データベースメタデータ。</returns>
        internal async Task<DatabaseMetadata> GetDatabaseAsync(string databaseId)
        {
            using JsonDocument document = await GetJsonAsync($"{API_BASE_URL}/databases/{Uri.EscapeDataString(databaseId)}");
            return DatabaseMetadata.FromJson(document.RootElement);
        }

        /// <summary>
        ///     指定データソースのスキーマを取得する。
        /// </summary>
        /// <param name="dataSourceId">データソースID。</param>
        /// <returns>データソーススキーマ。</returns>
        internal async Task<DataSourceSchema> GetDataSourceAsync(string dataSourceId)
        {
            using JsonDocument document = await GetJsonAsync($"{API_BASE_URL}/data_sources/{Uri.EscapeDataString(dataSourceId)}");
            return DataSourceSchema.FromJson(document.RootElement);
        }

        /// <summary>
        ///     指定データソースに含まれるページをページネーションしながら逐次取得する。
        /// </summary>
        /// <param name="dataSourceId">データソースID。</param>
        /// <returns>データソース内のページを返す非同期ストリーム。</returns>
        internal async IAsyncEnumerable<PageMetadata> QueryDataSourcePagesAsync(string dataSourceId)
        {
            string? nextCursor = null;

            do
            {
                Dictionary<string, object> requestBody = new()
                {
                    ["page_size"] = DATA_SOURCE_PAGE_SIZE
                };
                if (!string.IsNullOrWhiteSpace(nextCursor))
                {
                    requestBody["start_cursor"] = nextCursor;
                }

                string json = JsonSerializer.Serialize(requestBody);
                using JsonDocument document = await PostJsonAsync(
                    $"{API_BASE_URL}/data_sources/{Uri.EscapeDataString(dataSourceId)}/query",
                    json);
                JsonElement root = document.RootElement;
                if (root.TryGetProperty("results", out JsonElement results) && results.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement result in results.EnumerateArray())
                    {
                        if (result.TryGetProperty("object", out JsonElement objectElement) &&
                            objectElement.GetString() == "page")
                        {
                            yield return PageMetadata.FromJson(result);
                        }
                    }
                }

                bool hasMore = root.TryGetProperty("has_more", out JsonElement hasMoreElement) && hasMoreElement.GetBoolean();
                nextCursor = hasMore && root.TryGetProperty("next_cursor", out JsonElement cursorElement)
                    ? cursorElement.GetString()
                    : null;
            } while (!string.IsNullOrWhiteSpace(nextCursor));

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
        ///     Enhanced Markdown APIから単一のMarkdown断片を取得する。
        /// </summary>
        /// <param name="pageOrBlockId">ページまたはブロックID。</param>
        /// <returns>Markdown断片。</returns>
        private async Task<MarkdownFragment> GetMarkdownFragmentAsync(string pageOrBlockId)
        {
            using JsonDocument document = await GetJsonAsync(
                $"{API_BASE_URL}/pages/{Uri.EscapeDataString(pageOrBlockId)}/markdown");
            JsonElement root = document.RootElement;
            string markdown = root.TryGetProperty("markdown", out JsonElement markdownElement)
                ? markdownElement.GetString() ?? string.Empty
                : string.Empty;
            bool isTruncated = root.TryGetProperty("truncated", out JsonElement truncatedElement) &&
                               truncatedElement.GetBoolean();
            List<string> unknownBlockIds = new();
            if (root.TryGetProperty("unknown_block_ids", out JsonElement unknownElement) &&
                unknownElement.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement idElement in unknownElement.EnumerateArray())
                {
                    string? id = idElement.GetString();
                    if (!string.IsNullOrWhiteSpace(id)) { unknownBlockIds.Add(id); }
                }
            }

            return new MarkdownFragment(markdown, isTruncated, unknownBlockIds);
        }

        /// <summary>
        ///     GETリクエストを送信してJSONレスポンスを取得する。
        /// </summary>
        /// <param name="url">リクエストURL。</param>
        /// <returns>JSONドキュメント。</returns>
        private async Task<JsonDocument> GetJsonAsync(string url)
        {
            string responseBody = await SendAsync(HttpMethod.Get, url, null);
            return JsonDocument.Parse(responseBody);
        }

        /// <summary>
        ///     POSTリクエストを送信してJSONレスポンスを取得する。
        /// </summary>
        /// <param name="url">リクエストURL。</param>
        /// <param name="json">JSONリクエスト本文。</param>
        /// <returns>JSONドキュメント。</returns>
        private async Task<JsonDocument> PostJsonAsync(string url, string json)
        {
            string responseBody = await SendAsync(HttpMethod.Post, url, json);
            return JsonDocument.Parse(responseBody);
        }

        /// <summary>
        ///     Notion APIへリクエストを送信し、一時エラー時は待機して再試行する。
        /// </summary>
        /// <param name="method">HTTPメソッド。</param>
        /// <param name="url">リクエストURL。</param>
        /// <param name="json">任意のJSON本文。</param>
        /// <returns>レスポンス本文。</returns>
        private async Task<string> SendAsync(HttpMethod method, string url, string? json)
        {
            for (int attempt = 1; attempt <= MAX_RETRY_COUNT; attempt++)
            {
                using HttpRequestMessage request = new(method, url);
                if (json != null)
                {
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                using HttpResponseMessage response = await _httpClient.SendAsync(request);
                string responseBody = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode) { return responseBody; }

                bool canRetry = response.StatusCode == HttpStatusCode.TooManyRequests ||
                                (int)response.StatusCode >= 500;
                if (canRetry && attempt < MAX_RETRY_COUNT)
                {
                    TimeSpan waitTime = GetRetryDelay(response, attempt);
                    Console.WriteLine($"  Notion APIが混雑しています。{waitTime.TotalSeconds:0}秒後に再試行します。");
                    await Task.Delay(waitTime);
                    continue;
                }

                string error = responseBody.Length > 800 ? responseBody[..800] : responseBody;
                throw new NotionApiException(
                    response.StatusCode,
                    $"Notion APIが {(int)response.StatusCode} {response.ReasonPhrase} を返しました。{error}");
            }

            throw new InvalidOperationException("Notion APIへの再試行回数を超えました。");
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

        /// <summary>
        ///     単一のMarkdown APIレスポンスを保持するクラス。
        /// </summary>
        private sealed class MarkdownFragment
        {
            /// <summary>
            ///     Markdown断片を生成する。
            /// </summary>
            /// <param name="markdown">Markdown本文。</param>
            /// <param name="isTruncated">分割取得が必要かどうか。</param>
            /// <param name="unknownBlockIds">未取得ブロックID。</param>
            internal MarkdownFragment(
                string markdown,
                bool isTruncated,
                IReadOnlyList<string> unknownBlockIds)
            {
                Markdown = markdown;
                IsTruncated = isTruncated;
                UnknownBlockIds = unknownBlockIds;
            }

            internal string Markdown { get; }
            internal bool IsTruncated { get; }
            internal IReadOnlyList<string> UnknownBlockIds { get; }
        }
    }
}
