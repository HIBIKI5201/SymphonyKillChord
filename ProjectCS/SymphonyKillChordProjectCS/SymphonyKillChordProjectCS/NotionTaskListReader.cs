using Notion.Client;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SinfoniaStudio.SinfoniaOperator
{
    internal class NotionTaskListReader
    {
        public NotionTaskListReader(
            string notionToken,
            string databaseID,
            string datePropertyName,
            string namePropertyName)
        {
            _notionToken = notionToken;
            _databaseID = databaseID;
            _datePropertyName = datePropertyName;
            _namePropertyName = namePropertyName;
        }

        public async Task<string> GetTaskContent()
        {
            List<IWikiDatabase> database = await GetDatabaseAsync();

            // --- 日本時間を取得 ---
            DateTime nowTime = DateTime.UtcNow.AddHours(9);
            DateTime today = nowTime.Date;

            StringBuilder sb = new StringBuilder($"GitHub Actionsからの定期タスク通知です！ {nowTime:yyyy/MM/dd HH:mm:ss}");
            int taskCount = 0;

            PriorityQueue<StringBuilder, int> priorityQueue = new();

            // --- 開始タスク ---
            foreach (var item in database)
            {
                if (item is not Page page) continue;

                // 日付プロパティを取得できる場合。
                if (!page.Properties.TryGetValue(_datePropertyName, out PropertyValue? datePropertyValue) ||
                    datePropertyValue is not DatePropertyValue dateProperty) { continue; }

                // ページ名を取得。
                string pageName = GetPageName(page);

                #region 開始タスクの通知。
                DateTime? startDate = GetDateJSTTime(dateProperty.Date.Start);
                if (startDate.HasValue && startDate.Value.Date == today)
                {
                    taskCount++;
                    StringBuilder startTasksSb = new();
                    startTasksSb.AppendLine($"\n🟢 開始タスク: {pageName}\n[URL]({GetNotionPageUrl(page)})");

                    await AppendPageContentAsync(startTasksSb, page);
                    priorityQueue.Enqueue(startTasksSb, 0);
                    continue;
                }
                #endregion

                #region 納期タスクの通知。
                DateTime? endDate = GetDateJSTTime(dateProperty.Date.End);
                if (endDate.HasValue && endDate.Value.Date == today)
                {
                    taskCount++;
                    StringBuilder endTasksSb = new();

                    endTasksSb.AppendLine($"\n🔴 納期タスク: {pageName}\n[URL]({GetNotionPageUrl(page)})");
                    await AppendPageContentAsync(endTasksSb, page);
                    priorityQueue.Enqueue(endTasksSb, 1);
                    continue;
                }
                #endregion

                Console.WriteLine($"{pageName}は通知しません。");
            }

            if (taskCount <= 0)
            {
                Console.WriteLine("今日の開始タスクと納期タスクがありません。通知を送信しません。");
                return string.Empty;
            }

            // 優先度順にログを並べる。
            while (priorityQueue.TryDequeue(out StringBuilder? element, out int priority))
            {
                sb.AppendLine(element.ToString());
            }

            return sb.ToString();
        }

        private readonly string _notionToken;
        private readonly string _databaseID;
        private readonly string _datePropertyName;
        private readonly string _namePropertyName;

        private async Task AppendPageContentAsync(StringBuilder sb, Page page)
        {
            string pageContext = await GetBlockChildrenViaHttpAsync(page.Id);
            sb.AppendLine(new string('-', 10));
            sb.AppendLine(pageContext.TrimEnd());
            sb.AppendLine(new string('-', 10));
            sb.AppendLine();
        }

        private async Task<string> GetBlockChildrenViaHttpAsync(string blockId)
        {
            var sb = new StringBuilder();
            string? startCursor = null;
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _notionToken);
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
                                        sb.AppendLine(await GetBlockChildrenViaHttpAsync(childId));
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


        private async Task<List<IWikiDatabase>> GetDatabaseAsync()
        {
            NotionClient notion = NotionClientFactory.Create(new ClientOptions
            {
                AuthToken = _notionToken,
            });

            DatabaseQueryResponse query = await notion.Databases.QueryAsync(_databaseID, new DatabasesQueryParameters());
            List<IWikiDatabase> database = query.Results;

            if (database.Count == 0)
            {
                Console.WriteLine("データベースの要素がありません。");
                return new();
            }

            return database;
        }

        private string GetNotionPageUrl(Page page)
        {
            // page.Id は "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx" のような形式
            string rawId = page.Id.Replace("-", "");

            if (rawId.Length != 32)
                throw new InvalidOperationException("Notion Page ID が不正です。");

            // ハイフンを追加して Notion URL に適した形式に変換
            string formattedId = $"{rawId.Substring(0, 8)}-{rawId.Substring(8, 4)}-{rawId.Substring(12, 4)}-{rawId.Substring(16, 4)}-{rawId.Substring(20, 12)}";

            // ワークスペースは省略可能（そのまま https://www.notion.so/<formattedId> でアクセス可能）
            string url = $"https://www.notion.so/{formattedId}";

            return url;
        }

        private string GetPageName(Page page)
        {
            string pageName = "(名称未設定)";
            if (page.Properties.TryGetValue(_namePropertyName, out PropertyValue? titlePropValue) &&
                titlePropValue is TitlePropertyValue titleProperty)
            {
                pageName = string.Join("", titleProperty.Title.Select(t => t.PlainText));
            }

            return pageName;
        }

        private static DateTime? GetDateJSTTime(DateTimeOffset? offset) => offset?.UtcDateTime.AddHours(9);

    }
}
