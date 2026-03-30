using Notion.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SinfoniaStudio.SinfoniaOperator
{
    internal class NotionReader
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
                Console.WriteLine($"ページ内容の取得中にエラーが発生しました（PageId: {page.Id}）: {ex.Message}");
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
                        Console.WriteLine($"Notion API エラー: {resp.StatusCode} (BlockId: {blockId})");
                        break;
                    }

                    using JsonDocument doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
                    JsonElement root = doc.RootElement;

                    if (!root.TryGetProperty("results", out JsonElement results))
                    {
                        Console.WriteLine($"Notion API レスポンスに results がありません (BlockId: {blockId})");
                        break;
                    }

                    foreach (JsonElement block in results.EnumerateArray())
                    {
                        try
                        {
                            string type = block.GetProperty("type").GetString() ?? "unknown";

                            ConvertBlock(sb, type, block);

                            if (block.TryGetProperty("has_children", out JsonElement hasChildrenProp) && hasChildrenProp.ValueKind == JsonValueKind.True)
                            {
                                if (block.TryGetProperty("id", out JsonElement childIdEl))
                                {
                                    string? childId = childIdEl.GetString();
                                    if (!string.IsNullOrEmpty(childId))
                                    { 
                                        sb.AppendLine(await GetBlockChildrenAsync(childId)); 
                                    }
                                }
                            }
                        }
                        catch (Exception innerEx)
                        {
                            Console.WriteLine($"ブロック処理中にエラー（部分ブロック）: {innerEx.Message}");
                        }
                    }

                    startCursor = root.TryGetProperty("next_cursor", out var nextCursorEl) && nextCursorEl.ValueKind == JsonValueKind.String
                        ? nextCursorEl.GetString()
                        : null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ブロック取得中にエラーが発生しました（BlockId: {blockId}）: {ex.Message}");
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

            Console.WriteLine($"[NotionReader] データベースの取得を開始します (DatabaseID: {databaseID})");

            do
            {
                try
                {
                    pageCount++;
                    Console.WriteLine($"[NotionReader] クエリ実行中... (ページ: {pageCount})");

                    var requestData = new Dictionary<string, object>
                    {
                        { "page_size", 100 }
                    };
                    if (!string.IsNullOrEmpty(nextCursor))
                    {
                        requestData.Add("start_cursor", nextCursor);
                    }

                    string jsonBody = JsonSerializer.Serialize(requestData);
                    using var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                    HttpResponseMessage resp = await http.PostAsync($"https://api.notion.com/v1/databases/{databaseID}/query", content);
                    if (!resp.IsSuccessStatusCode)
                    {
                        string errorBody = await resp.Content.ReadAsStringAsync();
                        Console.WriteLine($"Notion API エラー: {resp.StatusCode} (DatabaseID: {databaseID}) - {errorBody}");
                        break;
                    }

                    string rawJson = await resp.Content.ReadAsStringAsync();
                    using JsonDocument doc = JsonDocument.Parse(rawJson);
                    JsonElement root = doc.RootElement;

                    if (root.TryGetProperty("results", out JsonElement results))
                    {
                        foreach (JsonElement pageEl in results.EnumerateArray())
                        {
                            try
                            {
                                // ページ単位でデシリアライズを試みる。
                                // 特定のページでアイコン(IPageIcon)のパースに失敗しても、ここでのキャッチにより無視して進める。
                                string singlePageJson = pageEl.GetRawText();
                                var page = Newtonsoft.Json.JsonConvert.DeserializeObject<Page>(singlePageJson);
                                if (page != null)
                                {
                                    allResults.Add((IWikiDatabase)page);
                                }
                            }
                            catch (Exception innerEx)
                            {
                                // カスタム絵文字など未対応の形式が含まれるページはスキップ。
                                Console.WriteLine($"[NotionReader] ページの取得をスキップしました (エラー: {innerEx.Message})");
                            }
                        }
                    }

                    nextCursor = root.TryGetProperty("next_cursor", out var nextCursorEl) && nextCursorEl.ValueKind == JsonValueKind.String
                        ? nextCursorEl.GetString()
                        : null;

                    Console.WriteLine($"[NotionReader] {allResults.Count} 件のアイテムを取得済み");

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[NotionReader] データベース取得中に重大なエラーが発生しました: {ex.Message}");
                    break;
                }

            } while (!string.IsNullOrEmpty(nextCursor));

            if (allResults.Count == 0)
            {
                Console.WriteLine($"[NotionReader] データベース {databaseID} は空、またはアクセス権限がありません。");
            }
            else
            {
                Console.WriteLine($"[NotionReader] データベースの全件取得が完了しました (合計: {allResults.Count} 件)");
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

        private static void ConvertBlock(StringBuilder sb, string type, JsonElement block)
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
                Console.WriteLine($"未対応のブロックタイプ: {type} (BlockId: {block.GetProperty("id").GetString()})");
                return;
            }

            sb.AppendLine(text);
        }

        private static string ExtractPlainTextFromRichTextArray(JsonElement richTextArray)
        {
            StringBuilder sb = new();
            if (richTextArray.ValueKind != JsonValueKind.Array) return string.Empty;
            foreach (JsonElement rt in richTextArray.EnumerateArray())
            {
                if (rt.TryGetProperty("plain_text", out JsonElement plainTextEl))
                    sb.Append(plainTextEl.GetString());
            }
            return sb.ToString();
        }

        private static string ConvertBlockParagraph(JsonElement block)
        {
            if (block.TryGetProperty(BLOCK_TYPE_PARAGRAPH, out var paragraphObj) &&
                paragraphObj.TryGetProperty("rich_text", out var pRt))
            {
                return ExtractPlainTextFromRichTextArray(pRt);
            }
            return string.Empty;
        }

        private static string ConvertBlockHeading(JsonElement block, string headingType)
        {
            if (block.TryGetProperty(headingType, out var headingObj) &&
                headingObj.TryGetProperty("rich_text", out var hRt))
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

        private static string ConvertBlockToDo(JsonElement block)
        {
            if (block.TryGetProperty(BLOCK_TYPE_TO_DO, out var todo) &&
                todo.TryGetProperty("rich_text", out var todoRt))
            {
                bool isChecked = todo.TryGetProperty("checked", out var checkedEl) && checkedEl.ValueKind == JsonValueKind.True;
                string checkbox = isChecked ? "[x]" : "[ ]";
                return $"{checkbox} {ExtractPlainTextFromRichTextArray(todoRt)}";
            }
            return string.Empty;
        }

        private static string ConvertBlockBulletedListItem(JsonElement block)
        {
            if (block.TryGetProperty(BLOCK_TYPE_BULLETED_LIST_ITEM, out var bullet) &&
                bullet.TryGetProperty("rich_text", out var bulletRt))
            {
                return $"・{ExtractPlainTextFromRichTextArray(bulletRt)}";
            }
            return string.Empty;
        }

        private static string ConvertBlockNumberedListItem(JsonElement block)
        {
            if (block.TryGetProperty(BLOCK_TYPE_NUMBERED_LIST_ITEM, out var num) &&
                num.TryGetProperty("rich_text", out var numRt))
            {
                return $"- {ExtractPlainTextFromRichTextArray(numRt)}";
            }
            return string.Empty;
        }

        private static string ConvertBlockQuote(JsonElement block)
        {
            if (block.TryGetProperty(BLOCK_TYPE_QUOTE, out var quote) &&
                quote.TryGetProperty("rich_text", out var quoteRt))
            {
                return $"> {ExtractPlainTextFromRichTextArray(quoteRt)}";
            }
            return string.Empty;
        }

        private static string ConvertBlockLinkPreview(JsonElement block)
        {
            if (block.TryGetProperty(BLOCK_TYPE_LINK_PREVIEW, out var linkPreviewObj) &&
                linkPreviewObj.TryGetProperty("url", out var urlEl))
            {
                string urlString = urlEl.GetString() ?? string.Empty;
                if (!string.IsNullOrEmpty(urlString))
                {
                    return $"[ページリンク]({urlString})";
                }
            }
            return string.Empty;
        }
    }
}
