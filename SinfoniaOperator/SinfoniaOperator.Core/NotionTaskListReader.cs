using Notion.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SinfoniaStudio.SinfoniaOperator
{
    /// <summary>
    ///     Notionのタスクデータベースを読み取り、通知用の文字列や一覧データを構築するクラス。
    /// </summary>
    public class NotionTaskListReader
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
        public async Task<TaskContentResult> GetTaskContent()
        {
            try
            {
                OperatorLog.Write($"[TaskReader] タスク情報の構築を開始します。 (Target Property: {_env.DatePropertyName}, Status: {_env.StatusPropertyName})");
                List<IWikiDatabase> database = await _reader.GetDatabaseAsync(_env.TaskDatabaseID);

                // 日本時間を取得。
                DateTime nowTime = DateTimeUtility.JstNow();
                DateTime today = nowTime.Date;
                OperatorLog.Write($"[TaskReader] 判定基準日 (JST): {today:yyyy/MM/dd}");

                List<(StringBuilder content, int priority)> outputTaskList = new();
                List<(StringBuilder content, int priority)> outputTaskAlertList = new();
                int evaluatedCount = 0;
                int notificationCount = 0;

                OperatorLog.Write($"{new string('-', 5)}タスクリスト ログ{new string('-', 5)}");

                foreach (IWikiDatabase item in database)
                {
                    evaluatedCount++;
                    if (item is not Page page) { continue; }

                    string pageName = NotionReader.GetPageName(page, _env.NamePropertyName);

                    if (!TryGetStatus(page, pageName, out string status)) { continue; }

                    // ステータスが完了済みなら終了。
                    if (_env.TaskDoneStatusName.Contains(status))
                    {
                        OperatorLog.Write($"[TaskReader] {pageName}: ステータスが{status}のためスキップします。");
                        continue;
                    }

                    if (!TryGetDateRange(page, pageName, out DateTime startDate, out DateTime endDate))
                    {
                        OperatorLog.Write($"[TaskReader] {pageName}: 開始日時がないため、通知しません。");
                        continue; // 開始日時がない場合は、通知しない。
                    }

                    #region 納期タスクの通知。
                    if (endDate.Date == today)
                    {
                        notificationCount++;
                        StringBuilder endTasksSb = new();
                        AppendTaskSummry(endTasksSb, "🟡納期タスク", pageName, page, startDate, endDate);
                        await AppendPageContentAsync(endTasksSb, page);
                        outputTaskList.Add((endTasksSb, 1));
                        OperatorLog.Write($"[TaskReader] {pageName}: 納期タスクとして通知リストに追加しました。");
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
                        outputTaskList.Add((startTasksSb, 0));
                        OperatorLog.Write($"[TaskReader] {pageName}: 開始タスクとして通知リストに追加しました。");
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
                        outputTaskAlertList.Add((endTasksSb, 2));
                        OperatorLog.Write($"[TaskReader] {pageName}: 納期遅れタスクとして通知リストに追加しました。");
                        continue;
                    }
                    #endregion

                    OperatorLog.Write($"[TaskReader] {pageName}: 通知対象外です。 (期間: {startDate:yyyy/MM/dd} ～ {endDate:yyyy/MM/dd})");
                }

                OperatorLog.Write($"[TaskReader] 評価終了 (評価数: {evaluatedCount}, 通知対象数: {notificationCount})");

                if (outputTaskList.Count <= 0 && outputTaskAlertList.Count <= 0)
                {
                    OperatorLog.Write("[TaskReader] 今日の開始タスクと納期タスクがありません。通知を送信しません。");
                    return new TaskContentResult(string.Empty, string.Empty, evaluatedCount, notificationCount);
                }

                // 優先度順にログを並べる。
                string taskLog = string.Empty;
                if (outputTaskList.Count > 0)
                {
                    StringBuilder taskLogSb = new($"[タスク通知] {nowTime:yyyy/MM/dd HH:mm:ss}\n");
                    foreach ((StringBuilder content, int priority) element in outputTaskList.OrderBy(e => e.priority))
                    {
                        taskLogSb.AppendLine(element.content.ToString());
                    }
                    taskLog = taskLogSb.ToString();
                }
                string taskAlertLog = string.Empty;
                if (outputTaskAlertList.Count > 0)
                {
                    StringBuilder taskAlertSb = new($"GitHub Actionsからの定期アラート通知です！ {nowTime:yyyy/MM/dd HH:mm:ss}");
                    foreach ((StringBuilder content, int priority) element in outputTaskAlertList.OrderBy(e => e.priority))
                    {
                        taskAlertSb.AppendLine(element.content.ToString());
                    }
                    taskAlertLog = taskAlertSb.ToString();
                }

                OperatorLog.Write($"{new string('-', 10)}");
                return new TaskContentResult(taskLog, taskAlertLog, evaluatedCount, notificationCount);
            }
            catch (Exception ex)
            {
                OperatorLog.Write($"[TaskReader] タスク情報の取得中に予期せぬエラーが発生しました: {ex.Message}");
                return new TaskContentResult(string.Empty, string.Empty, 0, 0);
            }
        }

        /// <summary>
        ///     Notionのデータベースにアクセスして、タスクの一覧を構造化データで返します。
        ///     ページ本文は取得しないため、一覧表示用途に向いています。
        /// </summary>
        /// <returns></returns>
        public async Task<List<NotionTaskItem>> GetTaskItemsAsync()
        {
            List<NotionTaskItem> items = new();
            List<IWikiDatabase> database = await _reader.GetDatabaseAsync(_env.TaskDatabaseID);

            // 日本時間を取得。
            DateTime today = DateTimeUtility.JstNow().Date;

            foreach (IWikiDatabase item in database)
            {
                if (item is not Page page) { continue; }

                string pageName = NotionReader.GetPageName(page, _env.NamePropertyName);

                if (!TryGetStatus(page, pageName, out string status)) { status = string.Empty; }

                bool hasDate = TryGetDateRange(page, pageName, out DateTime startDate, out DateTime endDate);

                // 通知区分を判定する。
                NotionTaskCategory category = NotionTaskCategory.None;
                if (_env.TaskDoneStatusName.Contains(status))
                {
                    category = NotionTaskCategory.Done;
                }
                else if (hasDate)
                {
                    if (endDate.Date == today) { category = NotionTaskCategory.Deadline; }
                    else if (startDate.Date == today) { category = NotionTaskCategory.Start; }
                    else if (endDate.Date < today) { category = NotionTaskCategory.Overdue; }
                }

                items.Add(new NotionTaskItem(
                    pageName,
                    status,
                    category,
                    hasDate,
                    startDate,
                    endDate,
                    page.Url ?? string.Empty,
                    page.PublicUrl ?? string.Empty));
            }

            return items;
        }

        public readonly struct TaskContentResult
        {
            public TaskContentResult(string taskContent, string taskAlertContent, int evaluatedCount, int notificationCount)
            {
                TaskContent = taskContent;
                TaskAlertContent = taskAlertContent;
                EvaluatedCount = evaluatedCount;
                NotificationCount = notificationCount;
            }

            public bool HasTaskContent => !string.IsNullOrWhiteSpace(TaskContent);
            public bool HasTaskAlertContent => !string.IsNullOrWhiteSpace(TaskAlertContent);

            public readonly string TaskContent;
            public readonly string TaskAlertContent;
            public readonly int EvaluatedCount;
            public readonly int NotificationCount;
        }

        private readonly NotionReader _reader;
        private readonly NotionEnvironment _env;

        /// <summary>
        ///     ページからステータス名を取得する。取得できない場合はfalseを返す。
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageName"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private bool TryGetStatus(Page page, string pageName, out string status)
        {
            status = string.Empty;

            if (page.Properties == null || !page.Properties.TryGetValue(_env.StatusPropertyName, out PropertyValue? statusPropertyValue))
            {
                OperatorLog.Write($"[TaskReader] {pageName}: ステータスプロパティ '{_env.StatusPropertyName}' が見つかりません。");
                return false;
            }
            if (statusPropertyValue is not StatusPropertyValue statusProperty)
            {
                OperatorLog.Write($"[TaskReader] {pageName}: ステータスプロパティ '{_env.StatusPropertyName}' の型が Status ではありません。");
                return false;
            }

            status = statusProperty.Status.Name;
            return true;
        }

        /// <summary>
        ///     ページから開始・終了日時（JST）を取得する。開始日時がない場合はfalseを返す。
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageName"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        private bool TryGetDateRange(Page page, string pageName, out DateTime startDate, out DateTime endDate)
        {
            startDate = default;
            endDate = default;

            if (page.Properties == null || !page.Properties.TryGetValue(_env.DatePropertyName, out PropertyValue? datePropertyValue))
            {
                OperatorLog.Write($"[TaskReader] {pageName}: プロパティ '{_env.DatePropertyName}' が見つかりません。");
                return false;
            }
            if (datePropertyValue is not DatePropertyValue dateProperty)
            {
                OperatorLog.Write($"[TaskReader] {pageName}: プロパティ '{_env.DatePropertyName}' の型が Date ではありません (型: {datePropertyValue?.Type.ToString() ?? "unknown"})。");
                return false;
            }

            if (dateProperty.Date == null || !DateTimeUtility.ConvertDateUtcToJst(dateProperty.Date.Start?.UtcDateTime, out startDate))
            {
                return false;
            }

            if (!DateTimeUtility.ConvertDateUtcToJst(dateProperty.Date.End?.UtcDateTime, out endDate))
            {
                endDate = startDate; // 終了日時がない場合は、開始日時と同じにする。
            }

            return true;
        }

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
