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
            try
            {
            List<IWikiDatabase> database = await _reader.GetDatabaseAsync(_databaseID);

            // 日本時間を取得。
            DateTime nowTime = DateTime.UtcNow.AddHours(9);
            DateTime today = nowTime.Date;

            PriorityQueue<StringBuilder, int> outputTaskQueue = new();

            Console.WriteLine($"{new string('-', 5)}タスクリスト ログ{new string('-', 5)}");

            foreach (IWikiDatabase item in database)
            {
                if (item is not Page page) { continue; }

                // 日付プロパティを取得できる場合。
                if (!page.Properties.TryGetValue(_datePropertyName, out PropertyValue? datePropertyValue) ||
                    datePropertyValue is not DatePropertyValue dateProperty) { continue; }
                // ステータスプロパティを取得できる場合。
                if (!page.Properties.TryGetValue(_statusPropertyName, out PropertyValue? statusPropertyValue) ||
                        statusPropertyValue is not StatusPropertyValue statusProperty) { continue; }

                // ステータスが完了済みなら終了。
                if (statusProperty.Status.Name == _taskStatusDoneName) { continue; }

                // ページ名を取得。
                string pageName = NotionReader.GetPageName(page, _namePropertyName);
                DateTime startDate = default;
                DateTime endDate = default;

                if (!ConvertDateUtcToJst(dateProperty.Date.Start?.UtcDateTime, out startDate))
                {
                    Console.WriteLine($"{pageName}は開始日時がないため、通知しません。");
                    continue; // 開始日時がない場合は、通知しない。
                }

                if (!ConvertDateUtcToJst(dateProperty.Date.End?.UtcDateTime, out endDate))
                {
                    endDate = startDate; // 終了日時がない場合は、開始日時と同じにする。
                }

                #region 納期タスクの通知。
                if (endDate.Date == today)
                {
                    StringBuilder endTasksSb = new();
                    AppendTaskSummry(endTasksSb, "🟡納期タスク", pageName, page, startDate, endDate);
                    await AppendPageContentAsync(endTasksSb, page);
                    outputTaskQueue.Enqueue(endTasksSb, 1);
                    Console.WriteLine($"{pageName}は納期タスク");
                    continue;
                }
                #endregion

                #region 開始タスクの通知。

                if (startDate.Date == today)
                {
                    StringBuilder startTasksSb = new();
                    AppendTaskSummry(startTasksSb, "🟢開始タスク", pageName, page, startDate, endDate);
                    await AppendPageContentAsync(startTasksSb, page);
                    outputTaskQueue.Enqueue(startTasksSb, 0);
                    Console.WriteLine($"{pageName}は開始タスク");
                    continue;
                }
                #endregion

                #region 納期遅れタスクの通知。
                if (endDate.Date < today)
                {
                    StringBuilder endTasksSb = new();
                    AppendTaskSummry(endTasksSb, "🔴納期遅れタスク", pageName, page, startDate, endDate);
                    await AppendPageContentAsync(endTasksSb, page);
                    outputTaskQueue.Enqueue(endTasksSb, 2);
                    Console.WriteLine($"{pageName}は納期遅れタスク");
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

            Console.WriteLine($"{new string('-', 10)}");
            return sb.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"タスク情報の取得中に予期せぬエラーが発生しました: {ex.Message}");
                return string.Empty;
            }
        }

        private readonly NotionReader _reader;
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
            string pageContext = await _reader.GetBlockChildrenViaHttpAsync(page.Id);
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

        private static bool ConvertDateUtcToJst(DateTime? utc, out DateTime jst)
        {
            if (utc == null)
            {
                jst = default;
                return false;
            }

            jst = utc.Value.AddHours(9);
            return true;
        }

    }
}
