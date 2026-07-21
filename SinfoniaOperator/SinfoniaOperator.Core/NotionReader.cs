using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Notion.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SinfoniaStudio.SinfoniaOperator
{
    /// <summary>
    ///     NotionのAPIからページやデータベースを取得・パースするクラス。
    /// </summary>
    public class NotionReader
    {
        public NotionReader(string notionToken)
        {
            _notionToken = notionToken;
        }

        /// <summary>
        ///     ページの中身をMarkdown文字列にする。
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public async Task<string> GetPageContentAsync(Page page)
        {
            try
            {
                return await GetPageMarkdownAsync(page.Id);
            }
            catch (Exception ex)
            {
                OperatorLog.Write($"ページ内容の取得中にエラーが発生しました（PageId: {page.Id}）: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        ///     Enhanced Markdown APIを使い、ページの内容をMarkdown文字列として取得する。
        ///     ページがAPI上限で分割された場合は、未取得ブロックを追加取得して連結する。
        /// </summary>
        /// <param name="pageOrBlockId"></param>
        /// <returns></returns>
        public async Task<string> GetPageMarkdownAsync(string pageOrBlockId)
        {
            using HttpClient http = new();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _notionToken);
            http.DefaultRequestHeaders.Add("Notion-Version", NOTION_MARKDOWN_API_VERSION);

            StringBuilder markdown = new();
            Queue<string> pendingIds = new();
            pendingIds.Enqueue(pageOrBlockId);
            HashSet<string> fetchedIds = new(StringComparer.OrdinalIgnoreCase);

            while (pendingIds.Count > 0)
            {
                string id = pendingIds.Dequeue();
                if (!fetchedIds.Add(id)) { continue; }

                try
                {
                    HttpResponseMessage resp = await http.GetAsync(
                        $"https://api.notion.com/v1/pages/{Uri.EscapeDataString(id)}/markdown");
                    if (!resp.IsSuccessStatusCode)
                    {
                        OperatorLog.Write($"Notion API エラー: {resp.StatusCode} (PageId: {id})");
                        continue;
                    }

                    JObject root = JObject.Parse(await resp.Content.ReadAsStringAsync());
                    string fragment = root["markdown"]?.ToString() ?? string.Empty;

                    if (markdown.Length > 0) { markdown.AppendLine(); }
                    markdown.Append(fragment);

                    bool isTruncated = root["truncated"]?.Type == JTokenType.Boolean && root["truncated"]!.Value<bool>();
                    if (isTruncated)
                    {
                        OperatorLog.Write($"[NotionReader] ページ {id} はAPI上限により分割取得されます。");
                    }

                    if (root["unknown_block_ids"] is JArray unknownIds)
                    {
                        foreach (JToken idToken in unknownIds)
                        {
                            string? childId = idToken.ToString();
                            if (!string.IsNullOrWhiteSpace(childId)) { pendingIds.Enqueue(childId); }
                        }
                    }
                }
                catch (Exception ex)
                {
                    OperatorLog.Write($"Markdown取得中にエラーが発生しました（PageId: {id}）: {ex.Message}");
                }
            }

            return markdown.ToString();
        }

        /// <summary>
        ///     Notionからデータベースの要素を取得する。
        /// </summary>
        /// <returns></returns>
        public async Task<List<IWikiDatabase>> GetDatabaseAsync(string databaseID)
        {
            databaseID = databaseID.Trim();
            List<IWikiDatabase> allResults = new();
            string? nextCursor = null;
            int pageCount = 0;

            using HttpClient http = new();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _notionToken);
            http.DefaultRequestHeaders.Add("Notion-Version", NOTION_API_VERSION);

            OperatorLog.Write($"[NotionReader] データベースの取得を開始します (DatabaseID: {databaseID})");

            do
            {
                try
                {
                    pageCount++;
                    OperatorLog.Write($"[NotionReader] クエリ実行中... (ページ: {pageCount})");

                    var requestData = new Dictionary<string, object>
                    {
                        { "page_size", 100 }
                    };
                    if (!string.IsNullOrEmpty(nextCursor))
                    {
                        requestData.Add("start_cursor", nextCursor);
                    }

                    string jsonBody = JsonConvert.SerializeObject(requestData);
                    using var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                    HttpResponseMessage resp = await http.PostAsync($"https://api.notion.com/v1/databases/{databaseID}/query", content);
                    if (!resp.IsSuccessStatusCode)
                    {
                        string errorBody = await resp.Content.ReadAsStringAsync();
                        OperatorLog.Write($"Notion API エラー: {resp.StatusCode} (DatabaseID: {databaseID}) - {errorBody}");
                        break;
                    }

                    string rawJson = await resp.Content.ReadAsStringAsync();
                    JObject root = JObject.Parse(rawJson);

                    if (root["results"] is JArray results)
                    {
                        foreach (JToken pageEl in results)
                        {
                            try
                            {
                                // ページ単位でデシリアライズを試みる。
                                if (pageEl is not JObject jo) { continue; }

                                // 未知のアイコン形式（custom_emoji等）は、ライブラリのデシリアライザが対応していないため、
                                // 事前にnullにしておくことでデシリアライズの失敗を防ぐ。
                                var icon = jo["icon"];
                                if (icon != null && icon.Type != JTokenType.Null)
                                {
                                    var type = icon["type"]?.ToString();
                                    if (type != "emoji" && type != "external" && type != "file")
                                    {
                                        jo["icon"] = null;
                                    }
                                }

                                var page = jo.ToObject<Page>();
                                if (page != null)
                                {
                                    allResults.Add((IWikiDatabase)page);
                                }
                            }
                            catch (Exception innerEx)
                            {
                                // それでもパースに失敗した場合はスキップ。
                                OperatorLog.Write($"[NotionReader] ページの取得をスキップしました (エラー: {innerEx.Message})");
                            }
                        }
                    }

                    nextCursor = root["next_cursor"]?.Type == JTokenType.String
                        ? root["next_cursor"]!.ToString()
                        : null;

                    OperatorLog.Write($"[NotionReader] {allResults.Count} 件のアイテムを取得済み");

                }
                catch (Exception ex)
                {
                    OperatorLog.Write($"[NotionReader] データベース取得中に重大なエラーが発生しました: {ex.Message}");
                    break;
                }

            } while (!string.IsNullOrEmpty(nextCursor));

            if (allResults.Count == 0)
            {
                OperatorLog.Write($"[NotionReader] データベース {databaseID} は空、またはアクセス権限がありません。");
            }
            else
            {
                OperatorLog.Write($"[NotionReader] データベースの全件取得が完了しました (合計: {allResults.Count} 件)");
            }

            return allResults;
        }

        /// <summary>
        ///     ページからページ名を取得する。
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public static string GetPageName(Page page, string namePropertyName)
        {
            const string DEFAULT_NAME = "(名称未設定)";
            if (page?.Properties == null) return DEFAULT_NAME;

            if (page.Properties.TryGetValue(namePropertyName, out PropertyValue? titlePropValue) &&
                titlePropValue is TitlePropertyValue titleProperty &&
                titleProperty.Title != null)
            {
                string name = string.Join("", titleProperty.Title
                    .Where(t => t != null && t.PlainText != null)
                    .Select(t => t.PlainText));

                return string.IsNullOrWhiteSpace(name) ? DEFAULT_NAME : name;
            }

            return DEFAULT_NAME;
        }

        private readonly string? _notionToken;

        /// <summary> データベースクエリ等、既存のブロックベースAPIで使用するバージョン。 </summary>
        private const string NOTION_API_VERSION = "2022-06-28";

        /// <summary> Enhanced Markdown APIで使用するバージョン。 </summary>
        private const string NOTION_MARKDOWN_API_VERSION = "2026-03-11";
    }
}
