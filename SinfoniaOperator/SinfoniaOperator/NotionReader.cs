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
        ///     タスクの中身を再帰的に文字列にする。
        /// </summary>
        /// <param name="blockId"></param>
        /// <returns></returns>
        public async Task<string> GetBlockChildrenViaHttpAsync(string blockId)
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
                                        sb.AppendLine(await GetBlockChildrenViaHttpAsync(childId)); 
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
            NotionClient notion = NotionClientFactory.Create(new ClientOptions
            {
                AuthToken = _notionToken,
            });

            DatabaseQueryResponse query = await notion.Databases.QueryAsync(databaseID, new DatabasesQueryParameters());
            List<IWikiDatabase> database = query.Results;

            if (database.Count == 0)
            {
                Console.WriteLine("データベースの要素がありません。");
                return new();
            }

            return database;
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
        private const string NOTION_API_VERSION = "2025-09-03";

        /// <summary>
        ///     ページからページ名を取得する。
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public static string GetPageName(Page page, string namePropertyName)
        {
            string pageName = "(名称未設定)";
            if (page.Properties.TryGetValue(namePropertyName, out PropertyValue? titlePropValue) &&
                titlePropValue is TitlePropertyValue titleProperty)
            {
                pageName = string.Join("", titleProperty.Title.Select(t => t.PlainText));
            }

            return pageName;
        }

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
