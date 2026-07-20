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
        ///     ページの中身を再帰的に文字列にする。
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public async Task<string> GetPageContentAsync(Page page)
        {
            try
            {
                return await GetBlockChildrenAsync(page.Id);
            }
            catch (Exception ex)
            {
                OperatorLog.Write($"ページ内容の取得中にエラーが発生しました（PageId: {page.Id}）: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        ///     ブロックの中身を再帰的に文字列にする。
        /// </summary>
        /// <param name="blockId"></param>
        /// <returns></returns>
        public async Task<string> GetBlockChildrenAsync(string blockId)
        {
            StringBuilder sb = new();
            string? startCursor = null;
            using HttpClient http = new();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _notionToken);
            http.DefaultRequestHeaders.Add("Notion-Version", NOTION_API_VERSION);

            do
            {
                try
                {
                    StringBuilder url = new($"https://api.notion.com/v1/blocks/{blockId}/children?page_size=100");
                    if (!string.IsNullOrEmpty(startCursor))
                    {
                        url.Append($"&start_cursor={Uri.EscapeDataString(startCursor)}");
                    }

                    HttpResponseMessage resp = await http.GetAsync(url.ToString());
                    if (!resp.IsSuccessStatusCode)
                    {
                        OperatorLog.Write($"Notion API エラー: {resp.StatusCode} (BlockId: {blockId})");
                        break;
                    }

                    JObject root = JObject.Parse(await resp.Content.ReadAsStringAsync());

                    if (root["results"] is not JArray results)
                    {
                        OperatorLog.Write($"Notion API レスポンスに results がありません (BlockId: {blockId})");
                        break;
                    }

                    foreach (JToken block in results)
                    {
                        try
                        {
                            string type = block["type"]?.ToString() ?? "unknown";

                            ConvertBlock(sb, type, block);

                            if (block["has_children"]?.Type == JTokenType.Boolean &&
                                block["has_children"]!.Value<bool>())
                            {
                                string? childId = block["id"]?.ToString();
                                if (!string.IsNullOrEmpty(childId))
                                {
                                    sb.AppendLine(await GetBlockChildrenAsync(childId));
                                }
                            }
                        }
                        catch (Exception innerEx)
                        {
                            OperatorLog.Write($"ブロック処理中にエラー（部分ブロック）: {innerEx.Message}");
                        }
                    }

                    startCursor = root["next_cursor"]?.Type == JTokenType.String
                        ? root["next_cursor"]!.ToString()
                        : null;
                }
                catch (Exception ex)
                {
                    OperatorLog.Write($"ブロック取得中にエラーが発生しました（BlockId: {blockId}）: {ex.Message}");
                    break;
                }

            } while (!string.IsNullOrEmpty(startCursor));

            return sb.ToString();
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

        private const string BLOCK_TYPE_PARAGRAPH = "paragraph";
        private const string BLOCK_TYPE_HEADING_1 = "heading_1";
        private const string BLOCK_TYPE_HEADING_2 = "heading_2";
        private const string BLOCK_TYPE_HEADING_3 = "heading_3";
        private const string BLOCK_TYPE_TO_DO = "to_do";
        private const string BLOCK_TYPE_BULLETED_LIST_ITEM = "bulleted_list_item";
        private const string BLOCK_TYPE_NUMBERED_LIST_ITEM = "numbered_list_item";
        private const string BLOCK_TYPE_QUOTE = "quote";
        private const string BLOCK_TYPE_LINK_PREVIEW = "link_preview";

        private readonly string? _notionToken;
        private const string NOTION_API_VERSION = "2022-06-28";

        private static void ConvertBlock(StringBuilder sb, string type, JToken block)
        {
            string? text = type switch
            {
                BLOCK_TYPE_PARAGRAPH => ConvertBlockParagraph(block),
                BLOCK_TYPE_HEADING_1 => ConvertBlockHeading(block, BLOCK_TYPE_HEADING_1),
                BLOCK_TYPE_HEADING_2 => ConvertBlockHeading(block, BLOCK_TYPE_HEADING_2),
                BLOCK_TYPE_HEADING_3 => ConvertBlockHeading(block, BLOCK_TYPE_HEADING_3),
                BLOCK_TYPE_TO_DO => ConvertBlockToDo(block),
                BLOCK_TYPE_BULLETED_LIST_ITEM => ConvertBlockBulletedListItem(block),
                BLOCK_TYPE_NUMBERED_LIST_ITEM => ConvertBlockNumberedListItem(block),
                BLOCK_TYPE_QUOTE => ConvertBlockQuote(block),
                BLOCK_TYPE_LINK_PREVIEW => ConvertBlockLinkPreview(block),
                _ => null
            };

            if (text == null)
            {
                OperatorLog.Write($"未対応のブロックタイプ: {type} (BlockId: {block["id"]?.ToString()})");
                return;
            }

            sb.AppendLine(text);
        }

        private static string ExtractPlainTextFromRichTextArray(JToken? richTextArray)
        {
            StringBuilder sb = new();
            if (richTextArray is not JArray array) return string.Empty;
            foreach (JToken rt in array)
            {
                var plainText = rt["plain_text"];
                if (plainText != null && plainText.Type == JTokenType.String)
                    sb.Append(plainText.ToString());
            }
            return sb.ToString();
        }

        private static string ConvertBlockParagraph(JToken block)
        {
            var pRt = block[BLOCK_TYPE_PARAGRAPH]?["rich_text"];
            if (pRt != null)
            {
                return ExtractPlainTextFromRichTextArray(pRt);
            }
            return string.Empty;
        }

        private static string ConvertBlockHeading(JToken block, string headingType)
        {
            var hRt = block[headingType]?["rich_text"];
            if (hRt != null)
            {
                string prefix = headingType switch
                {
                    BLOCK_TYPE_HEADING_1 => "# ",
                    BLOCK_TYPE_HEADING_2 => "## ",
                    BLOCK_TYPE_HEADING_3 => "### ",
                    _ => string.Empty
                };
                return $"{prefix}{ExtractPlainTextFromRichTextArray(hRt)}";
            }
            return string.Empty;
        }

        private static string ConvertBlockToDo(JToken block)
        {
            var todo = block[BLOCK_TYPE_TO_DO];
            var todoRt = todo?["rich_text"];
            if (todo != null && todoRt != null)
            {
                bool isChecked = todo["checked"]?.Type == JTokenType.Boolean && todo["checked"]!.Value<bool>();
                string checkbox = isChecked ? "[x]" : "[ ]";
                return $"{checkbox} {ExtractPlainTextFromRichTextArray(todoRt)}";
            }
            return string.Empty;
        }

        private static string ConvertBlockBulletedListItem(JToken block)
        {
            var bulletRt = block[BLOCK_TYPE_BULLETED_LIST_ITEM]?["rich_text"];
            if (bulletRt != null)
            {
                return $"・{ExtractPlainTextFromRichTextArray(bulletRt)}";
            }
            return string.Empty;
        }

        private static string ConvertBlockNumberedListItem(JToken block)
        {
            var numRt = block[BLOCK_TYPE_NUMBERED_LIST_ITEM]?["rich_text"];
            if (numRt != null)
            {
                return $"- {ExtractPlainTextFromRichTextArray(numRt)}";
            }
            return string.Empty;
        }

        private static string ConvertBlockQuote(JToken block)
        {
            var quoteRt = block[BLOCK_TYPE_QUOTE]?["rich_text"];
            if (quoteRt != null)
            {
                return $"> {ExtractPlainTextFromRichTextArray(quoteRt)}";
            }
            return string.Empty;
        }

        private static string ConvertBlockLinkPreview(JToken block)
        {
            var urlEl = block[BLOCK_TYPE_LINK_PREVIEW]?["url"];
            if (urlEl != null)
            {
                string urlString = urlEl.ToString();
                if (!string.IsNullOrEmpty(urlString))
                {
                    return $"[ページリンク]({urlString})";
                }
            }
            return string.Empty;
        }
    }
}
