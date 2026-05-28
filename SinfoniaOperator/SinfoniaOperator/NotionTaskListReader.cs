using Notion.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SinfoniaStudio.SinfoniaOperator
{
    internal class NotionTaskListReader
    {
        public NotionTaskListReader(NotionEnvironment env)
        {
            _reader = new NotionReader(env.NotionToken);
            _env = env;
        }

        /// <summary>
        ///     Notionのデータベースにアクセスして、タスクの状況を文字列で返します。
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetTaskContent()
        {
            try
            {
                Console.WriteLine($"[TaskReader] タスク情報の構築を開始します。 (Target Property: {_env.DatePropertyName}, Status: {_env.StatusPropertyName})");
                List<IWikiDatabase> database = await _reader.GetDatabaseAsync(_env.TaskDatabaseID);

                // 日本時間を取得。
                DateTime nowTime = DateTimeUtility.JstNow();
                DateTime today = nowTime.Date;
                Console.WriteLine($"[TaskReader] 判定基準日 (JST): {today:yyyy/MM/dd}");

                PriorityQueue<StringBuilder, int> outputTaskQueue = new();
                int evaluatedCount = 0;
                int notificationCount = 0;

                Console.WriteLine($"{new string('-', 5)}タスクリスト ログ{new string('-', 5)}");

                foreach (IWikiDatabase item in database)
                {
                    evaluatedCount++;
                    if (item is not Page page) { continue; }

                    string pageName = NotionReader.GetPageName(page, _env.NamePropertyName);

                    // 日付プロパティを取得できる場合。
                    if (page.Properties == null || !page.Properties.TryGetValue(_env.DatePropertyName, out PropertyValue? datePropertyValue))
                    {
                        Console.WriteLine($"[TaskReader] {pageName}: プロパティ '{_env.DatePropertyName}' が見つかりません。");
                        continue;
                    }
                    if (datePropertyValue is not DatePropertyValue dateProperty)
                    {
                        Console.WriteLine($"[TaskReader] {pageName}: プロパティ '{_env.DatePropertyName}' の型が Date ではありません (型: {datePropertyValue?.Type.ToString() ?? "unknown"})。");
                        continue;
                    }

                    // ステータスプロパティを取得できる場合。
                    if (!page.Properties.TryGetValue(_env.StatusPropertyName, out PropertyValue? statusPropertyValue))
                    {
                        Console.WriteLine($"[TaskReader] {pageName}: ステータスプロパティ '{_env.StatusPropertyName}' が見つかりません。");
                        continue;
                    }
                    if (statusPropertyValue is not StatusPropertyValue statusProperty)
                    {
                        Console.WriteLine($"[TaskReader] {pageName}: ステータスプロパティ '{_env.StatusPropertyName}' の型が Status ではありません。");
                        continue;
                    }

                    string status = statusProperty.Status.Name;
                    // ステータスが完了済みなら終了。
                    if (_env.TaskDoneStatusName.Contains(status))
                    {
                        Console.WriteLine($"[TaskReader] {pageName}: ステータスが{status}のためスキップします。");
                        continue;
                    }

                    DateTime startDate = default;
                    DateTime endDate = default;

                    if (dateProperty.Date == null || !DateTimeUtility.ConvertDateUtcToJst(dateProperty.Date.Start?.UtcDateTime, out startDate))
                    {
                        Console.WriteLine($"[TaskReader] {pageName}: 開始日時がないため、通知しません。");
                        continue; // 開始日時がない場合は、通知しない。
                    }

                    if (!DateTimeUtility.ConvertDateUtcToJst(dateProperty.Date.End?.UtcDateTime, out endDate))
                    {
                        endDate = startDate; // 終了日時がない場合は、開始日時と同じにする。
                    }

                    #region 納期タスクの通知。
                    if (endDate.Date == today)
                    {
                        notificationCount++;
                        StringBuilder endTasksSb = new();
                        AppendTaskSummry(endTasksSb, "🟡納期タスク", pageName, page, startDate, endDate);
                        await AppendPageContentAsync(endTasksSb, page);
                        outputTaskQueue.Enqueue(endTasksSb, 1);
                        Console.WriteLine($"[TaskReader] {pageName}: 納期タスクとして通知リストに追加しました。");
                        continue;
                    }
                    #endregion

                    #region 開始タスクの通知。

                    if (startDate.Date == today)
                    {
                        notificationCount++;
                        StringBuilder startTasksSb = new();
                        AppendTaskSummry(startTasksSb, "🟢開始タスク", pageName, page, startDate, endDate);
                        await AppendPageContentAsync(startTasksSb, page);
                        outputTaskQueue.Enqueue(startTasksSb, 0);
                        Console.WriteLine($"[TaskReader] {pageName}: 開始タスクとして通知リストに追加しました。");
                        continue;
                    }
                    #endregion

                    #region 納期遅れタスクの通知。
                    if (endDate.Date < today)
                    {
                        notificationCount++;
                        StringBuilder endTasksSb = new();
                        AppendTaskSummry(endTasksSb, "🔴納期遅れタスク", pageName, page, startDate, endDate);
                        await AppendPageContentAsync(endTasksSb, page);
                        outputTaskQueue.Enqueue(endTasksSb, 2);
                        Console.WriteLine($"[TaskReader] {pageName}: 納期遅れタスクとして通知リストに追加しました。");
                        continue;
                    }
                    #endregion

                    Console.WriteLine($"[TaskReader] {pageName}: 通知対象外です。 (期間: {startDate:yyyy/MM/dd} ～ {endDate:yyyy/MM/dd})");
                }

                Console.WriteLine($"[TaskReader] 評価終了 (評価数: {evaluatedCount}, 通知対象数: {notificationCount})");

                if (outputTaskQueue.Count <= 0)
                {
                    Console.WriteLine("[TaskReader] 今日の開始タスクと納期タスクがありません。通知を送信しません。");
                    return string.Empty;
                }

                // 優先度順にログを並べる。
                StringBuilder sb =
                    new($"GitHub Actionsからの定期タスク通知です！ {nowTime:yyyy/MM/dd HH:mm:ss}");
                while (outputTaskQueue.TryDequeue(out StringBuilder? element, out int priority))
                {
                    sb.AppendLine(element.ToString());
                }

                Console.WriteLine($"{new string('-', 10)}");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TaskReader] タスク情報の取得中に予期せぬエラーが発生しました: {ex.Message}");
                return string.Empty;
            }
        }

        private readonly NotionReader _reader;
        private readonly NotionEnvironment _env;

        /// <summary>
        ///     タスクのデータを取得する。
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        private async Task AppendPageContentAsync(StringBuilder sb, Page page)
        {
            string pageContext = await _reader.GetPageContentAsync(page);
            sb.AppendLine(new string('-', 10));
            sb.AppendLine(pageContext.TrimEnd());
            sb.AppendLine(new string('-', 10));
            sb.AppendLine();
        }

        private static void AppendTaskSummry(StringBuilder sb, string title, string pageName, Page page, DateTime startDate, DateTime endDate)
        {
            sb.AppendLine($"\n{title}: {pageName}\n[URL]({page.PublicUrl}) [編集]({page.Url})");
            sb.AppendLine($"開始日時: {startDate:yyyy/MM/dd} 終了日時: {endDate:yyyy/MM/dd}");
        }
    }
}
