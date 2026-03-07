using Notion.Client;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Net.Http;

namespace SinfoniaStudio.SinfoniaOperator
{
    internal class NotionTaskListReader
    {
        public NotionTaskListReader(NotionEnvironment env)
        {
            _notionToken = env.NotionToken;
            _databaseID = env.DatabaseID;
            _datePropertyName = env.DatePropertyName;
            _namePropertyName = env.NamePropertyName;
            _statusPropertyName = env.StatusPropertyName;
            _taskStatusDoneName = env.TaskDoneStatusName;
        }

        /// <summary>
        ///     Notionのデータベースにアクセスして、タスクの状況を文字列で返します。
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetTaskContent()
        {
            List<IWikiDatabase> database = await GetDatabaseAsync();

            // 日本時間を取得。
            DateTime nowTime = DateTime.UtcNow.AddHours(9);
            DateTime today = nowTime.Date;

            PriorityQueue<StringBuilder, int> outputTaskQueue = new();

            foreach (IWikiDatabase item in database)
            {
                if (item is not Page page) continue;

                // 日付プロパティを取得できる場合。
                if (!page.Properties.TryGetValue(_datePropertyName, out PropertyValue? datePropertyValue) ||
                    datePropertyValue is not DatePropertyValue dateProperty) { continue; }
                // ステータスプロパティを取得できる場合。
                if (!page.Properties.TryGetValue(_statusPropertyName, out PropertyValue? statusPropertyValue) ||
                        statusPropertyValue is not StatusPropertyValue statusProperty) { continue; }

                // ステータスが完了済みなら終了。
                if (statusProperty.Status.Name == _taskStatusDoneName) { continue; }

                // ページ名を取得。
                string pageName = GetPageName(page);
                DateTime? startDate = ConvertDateUtcToJst(dateProperty.Date.Start?.UtcDateTime);
                DateTime? endDate = ConvertDateUtcToJst(dateProperty.Date.End?.UtcDateTime);

                #region 開始タスクの通知。


                if (startDate.HasValue && startDate.Value.Date == today)
                {
                    StringBuilder startTasksSb = new();
                    startTasksSb.AppendLine($"\n🟢 開始タスク: {pageName}\n[URL]({page.PublicUrl}) [編集]({page.Url})");

                    await AppendPageContentAsync(startTasksSb, page);
                    outputTaskQueue.Enqueue(startTasksSb, 0);
                    continue;
                }
                #endregion

                #region 納期タスクの通知。
                if (endDate.HasValue && endDate.Value.Date == today)
                {
                    StringBuilder endTasksSb = new();

                    endTasksSb.AppendLine($"\n🟡 納期タスク: {pageName}\n[確認]({page.PublicUrl}) [編集]({page.Url})");
                    await AppendPageContentAsync(endTasksSb, page);
                    outputTaskQueue.Enqueue(endTasksSb, 1);
                    continue;
                }
                #endregion

                #region 納期遅れタスクの通知。
                if (endDate.HasValue && endDate.Value.Date < today)
                {
                    StringBuilder endTasksSb = new();

                    endTasksSb.AppendLine($"\n🔴 納期遅れタスク: {pageName}\n[確認]({page.PublicUrl}) [編集]({page.Url})");
                    await AppendPageContentAsync(endTasksSb, page);
                    outputTaskQueue.Enqueue(endTasksSb, 2);
                }
                #endregion

                Console.WriteLine($"{pageName}は通知しません。");
            }

            if (outputTaskQueue.Count <= 0)
            {
                Console.WriteLine("今日の開始タスクと納期タスクがありません。通知を送信しません。");
                return string.Empty;
            }

            // 優先度順にログを並べる。
            StringBuilder sb =
                new($"GitHub Actionsからの定期タスク通知です！ {nowTime:yyyy/MM/dd HH:mm:ss}");
            while (outputTaskQueue.TryDequeue(out StringBuilder? element, out int priority))
            {
                sb.AppendLine(element.ToString());
            }

            return sb.ToString();
        }

        private const string NOTION_API_VERSION = "2025-09-03";

        private readonly string _notionToken;
        private readonly string _databaseID;
        private readonly string _datePropertyName;
        private readonly string _namePropertyName;
        private readonly string _statusPropertyName;
        private readonly string _taskStatusDoneName;

        /// <summary>
        ///     タスクのデータを取得する。
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        private async Task AppendPageContentAsync(StringBuilder sb, Page page)
        {
            string pageContext = await GetBlockChildrenViaHttpAsync(page.Id);
            sb.AppendLine(new string('-', 10));
            sb.AppendLine(pageContext.TrimEnd());
            sb.AppendLine(new string('-', 10));
            sb.AppendLine();
        }

        /// <summary>
        ///     タスクの中身を再帰的に文字列にする。
        /// </summary>
        /// <param name="blockId"></param>
        /// <returns></returns>
        private async Task<string> GetBlockChildrenViaHttpAsync(string blockId)
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
                        url.Append($"&start_cursor={Uri.EscapeDataString(startCursor)}");

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

                            static string ExtractPlainTextFromRichTextArray(JsonElement richTextArray)
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

                                case "link_preview":
                                    if (block.TryGetProperty("link_preview", out var linkPreviewObj) &&
                                        linkPreviewObj.TryGetProperty("url", out var urlEl))
                                    {
                                        string urlString = urlEl.GetString() ?? string.Empty;
                                        if (!string.IsNullOrEmpty(urlString))
                                        {
                                            sb.AppendLine($"[ページリンク]({urlString})");
                                        }
                                    }
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

        /// <summary>
        ///     Notionからデータベースの要素を取得する。
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        ///     ページからページ名を取得する。
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
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

        private static DateTime? ConvertDateUtcToJst(DateTime? utc) => utc?.AddHours(9);

    }
}
