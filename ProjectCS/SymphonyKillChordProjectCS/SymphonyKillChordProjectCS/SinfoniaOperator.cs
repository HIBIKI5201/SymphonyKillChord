using Notion.Client;
using System.Text;
using System.Text.Json;

namespace SinfoniaStudio.Master
{
    internal class SinfoniaOperator
    {
        private const string DISCORD_WEBHOOK = "DISCORD_WEBHOOK";
        private const string NOTION_TOKEN = "NOTION_TOKEN";
        private const string NOTION_DATABASE_ID = "NOTION_DATABASE_ID";
        private const string NOTION_DATABASE_DATE_PROPERTY = "NOTION_DATABASE_DATE_PROPERTY";

        static async Task Main()
        {
            // --- GitHub Actions の Secrets から環境変数を取得 ---
            string webhookUrl = Environment.GetEnvironmentVariable(DISCORD_WEBHOOK) ?? string.Empty;
            string notionToken = Environment.GetEnvironmentVariable(NOTION_TOKEN) ?? string.Empty;
            string databaseID = Environment.GetEnvironmentVariable(NOTION_DATABASE_ID) ?? string.Empty;
            string datePropertyName = Environment.GetEnvironmentVariable(NOTION_DATABASE_DATE_PROPERTY) ?? string.Empty;

            if (string.IsNullOrEmpty(webhookUrl))
            {
                Console.WriteLine("環境変数 DISCORD_WEBHOOK が設定されていません。");
                return;
            }
            if (string.IsNullOrEmpty(notionToken))
            {
                Console.WriteLine("環境変数 NOTION_TOKEN が設定されていません。");
                return;
            }
            if (string.IsNullOrEmpty(databaseID))
            {
                Console.WriteLine("環境変数 NOTION_DATABASE_ID が設定されていません。");
                return;
            }
            if (string.IsNullOrEmpty(datePropertyName))
            {
                Console.WriteLine("環境変数 NOTION_DATABASE_DATE_PROPERTY が設定されていません。");
                return;
            }

            // --- Notion クライアント作成 ---
            NotionClient notion = NotionClientFactory.Create(new ClientOptions
            {
                AuthToken = notionToken,
            });

            // --- データベースをクエリ ---
            var query = await notion.Databases.QueryAsync(databaseID, new DatabasesQueryParameters());
            List<IWikiDatabase> database = query.Results;

            if (database.Count == 0)
            {
                Console.WriteLine("データベースの要素がありません。");
                return;
            }

            // --- 日本時間を取得 ---
            DateTime nowTime = DateTime.UtcNow.AddHours(9);
            DateTime today = nowTime.Date;

            StringBuilder sb = new StringBuilder($"GitHub Actionsからの定期タスク通知です！ {nowTime:yyyy/MM/dd HH:mm:ss}");
            int startTaskCount = 0;
            int endTaskCount = 0;

            StringBuilder startTasksSb = new();
            StringBuilder endTasksSb = new();

            // --- 開始タスク ---
            foreach (var item in database)
            {
                if (item is not Page page) continue;

                if (page.Properties.TryGetValue(datePropertyName, out var datePropertyValue) &&
                    datePropertyValue is DatePropertyValue dateProperty)
                {
                    DateTimeOffset? startOffset = dateProperty.Date?.Start;
                    DateTime? start = startOffset?.UtcDateTime.AddHours(9);

                    string pageName = "(名称未設定)";
                    if (page.Properties.TryGetValue("名前", out var titlePropValue) &&
                        titlePropValue is TitlePropertyValue titleProperty)
                    {
                        pageName = string.Join("", titleProperty.Title.Select(t => t.PlainText));
                    }

                    if (start.HasValue && start.Value.Date == today)
                    {
                        startTasksSb.AppendLine($"\n🟢 開始タスク: {pageName}");
                        startTaskCount++;

                        // ページ本文を追加
                        await AppendPageContentAsync(startTasksSb, page, notionToken);
                    }
                }
            }

            // --- 納期タスク ---
            foreach (var item in database)
            {
                if (item is not Page page) continue;

                if (page.Properties.TryGetValue(datePropertyName, out var datePropertyValue) &&
                    datePropertyValue is DatePropertyValue dateProperty)
                {
                    DateTimeOffset? endOffset = dateProperty.Date?.End;
                    DateTime? end = endOffset?.UtcDateTime.AddHours(9);

                    string pageName = "(名称未設定)";
                    if (page.Properties.TryGetValue("名前", out var titlePropValue) &&
                        titlePropValue is TitlePropertyValue titleProperty)
                    {
                        pageName = string.Join("", titleProperty.Title.Select(t => t.PlainText));
                    }

                    if (end.HasValue && end.Value.Date == today)
                    {
                        endTasksSb.AppendLine($"\n🔴 納期タスク: {pageName}");
                        endTaskCount++;

                        // ページ本文を追加
                        await AppendPageContentAsync(endTasksSb, page, notionToken);
                    }
                }
            }

            if (startTaskCount == 0 && endTaskCount == 0)
            {
                Console.WriteLine("今日の開始タスクと納期タスクがありません。通知を送信しません。");
                return;
            }

            sb.Append(startTasksSb);
            sb.Append(endTasksSb);

            // --- Discordへ送信 ---
            using var client = new HttpClient();
            var payload = new { content = sb.ToString() };
            var json = JsonSerializer.Serialize(payload);
            var response = await client.PostAsync(
                webhookUrl,
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            Console.WriteLine($"Discord送信結果: {response.StatusCode}");
        }

        private static async Task AppendPageContentAsync(StringBuilder sb, Page page, string notionToken)
        {
            string pageContext = await GetBlockChildrenViaHttpAsync(page.Id, notionToken);
            sb.AppendLine(new string('-', 10));
            sb.AppendLine(pageContext.TrimEnd());
            sb.AppendLine(new string('-', 10));
            sb.AppendLine();
        }

        private static async Task<string> GetBlockChildrenViaHttpAsync(string blockId, string notionToken)
        {
            var sb = new StringBuilder();
            string? startCursor = null;
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", notionToken);
            http.DefaultRequestHeaders.Add("Notion-Version", "2025-09-03");

            do
            {
                try
                {
                    var url = new StringBuilder($"https://api.notion.com/v1/blocks/{blockId}/children?page_size=100");
                    if (!string.IsNullOrEmpty(startCursor))
                        url.Append($"&start_cursor={Uri.EscapeDataString(startCursor)}");

                    var resp = await http.GetAsync(url.ToString());
                    if (!resp.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Notion API エラー: {resp.StatusCode} (BlockId: {blockId})");
                        break;
                    }

                    using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
                    var root = doc.RootElement;

                    if (!root.TryGetProperty("results", out var results))
                    {
                        Console.WriteLine($"Notion API レスポンスに results がありません (BlockId: {blockId})");
                        break;
                    }

                    foreach (var block in results.EnumerateArray())
                    {
                        try
                        {
                            string type = block.GetProperty("type").GetString() ?? "unknown";

                            static string ExtractPlainTextFromRichTextArray(JsonElement richTextArray)
                            {
                                var sb = new StringBuilder();
                                if (richTextArray.ValueKind != JsonValueKind.Array) return string.Empty;
                                foreach (var rt in richTextArray.EnumerateArray())
                                {
                                    if (rt.TryGetProperty("plain_text", out var plainTextEl))
                                        sb.Append(plainTextEl.GetString());
                                }
                                return sb.ToString();
                            }

                            switch (type)
                            {
                                case "paragraph":
                                    if (block.TryGetProperty("paragraph", out var paragraphObj) &&
                                        paragraphObj.TryGetProperty("rich_text", out var pRt))
                                        sb.AppendLine(ExtractPlainTextFromRichTextArray(pRt));
                                    break;

                                case "heading_1":
                                    if (block.TryGetProperty("heading_1", out var h1) &&
                                        h1.TryGetProperty("rich_text", out var h1Rt))
                                        sb.AppendLine($"# {ExtractPlainTextFromRichTextArray(h1Rt)}");
                                    break;

                                case "heading_2":
                                    if (block.TryGetProperty("heading_2", out var h2) &&
                                        h2.TryGetProperty("rich_text", out var h2Rt))
                                        sb.AppendLine($"## {ExtractPlainTextFromRichTextArray(h2Rt)}");
                                    break;

                                case "heading_3":
                                    if (block.TryGetProperty("heading_3", out var h3) &&
                                        h3.TryGetProperty("rich_text", out var h3Rt))
                                        sb.AppendLine($"### {ExtractPlainTextFromRichTextArray(h3Rt)}");
                                    break;

                                case "to_do":
                                    if (block.TryGetProperty("to_do", out var todo) &&
                                        todo.TryGetProperty("rich_text", out var todoRt))
                                    {
                                        bool isChecked = todo.TryGetProperty("checked", out var checkedEl) && checkedEl.ValueKind == JsonValueKind.True;
                                        string checkbox = isChecked ? "[x]" : "[ ]";
                                        sb.AppendLine($"{checkbox} {ExtractPlainTextFromRichTextArray(todoRt)}");
                                    }
                                    break;

                                case "bulleted_list_item":
                                    if (block.TryGetProperty("bulleted_list_item", out var bullet) &&
                                        bullet.TryGetProperty("rich_text", out var bulletRt))
                                        sb.AppendLine($"・{ExtractPlainTextFromRichTextArray(bulletRt)}");
                                    break;

                                case "numbered_list_item":
                                    if (block.TryGetProperty("numbered_list_item", out var num) &&
                                        num.TryGetProperty("rich_text", out var numRt))
                                        sb.AppendLine($"- {ExtractPlainTextFromRichTextArray(numRt)}");
                                    break;

                                case "quote":
                                    if (block.TryGetProperty("quote", out var quote) &&
                                        quote.TryGetProperty("rich_text", out var quoteRt))
                                        sb.AppendLine($"> {ExtractPlainTextFromRichTextArray(quoteRt)}");
                                    break;

                                default:
                                    sb.AppendLine($"[未対応ブロック: {type}]");
                                    break;
                            }

                            if (block.TryGetProperty("has_children", out var hasChildrenProp) && hasChildrenProp.ValueKind == JsonValueKind.True)
                            {
                                if (block.TryGetProperty("id", out var childIdEl))
                                {
                                    var childId = childIdEl.GetString();
                                    if (!string.IsNullOrEmpty(childId))
                                        sb.AppendLine(await GetBlockChildrenViaHttpAsync(childId, notionToken));
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
    }
}