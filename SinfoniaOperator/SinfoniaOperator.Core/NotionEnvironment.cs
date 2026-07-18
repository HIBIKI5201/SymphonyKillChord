using System;
using System.Linq;
using System.Text;

namespace SinfoniaStudio.SinfoniaOperator
{
    /// <summary>
    ///     Notionへのアクセスに必要な設定値を保持する構造体。
    /// </summary>
    public readonly struct NotionEnvironment
    {
        public NotionEnvironment(
            string notionToken,
            string taskDatabaseID,
            string sprintDatabaseID,
            string datePropertyName,
            string namePropertyName,
            string statusPropertyName,
            string[] taskDoneStatusName)
        {
            NotionToken = notionToken;
            TaskDatabaseID = taskDatabaseID;
            SprintDatabaseID = sprintDatabaseID;
            DatePropertyName = datePropertyName;
            NamePropertyName = namePropertyName;
            StatusPropertyName = statusPropertyName;
            TaskDoneStatusName = taskDoneStatusName;
        }

        /// <summary>
        ///     設定値（JSON設定または環境変数）を読み込んで構築する。
        ///     必要な設定値が見つからない場合は例外を投げる。
        /// </summary>
        public static NotionEnvironment FromConfig(
            string notionTokenKey,
            string taskDatabaseIDKey,
            string sprintDatabaseIDKey,
            string datePropertyNameKey,
            string namePropertyNameKey,
            string statusPropertyNameKey,
            string taskDoneStatusNameKey)
        {
            EnvironmentVariable notionToken = new(notionTokenKey);
            EnvironmentVariable taskDatabaseID = new(taskDatabaseIDKey);
            EnvironmentVariable sprintDatabaseID = new(sprintDatabaseIDKey);
            EnvironmentVariable datePropertyName = new(datePropertyNameKey);
            EnvironmentVariable namePropertyName = new(namePropertyNameKey);
            EnvironmentVariable statusPropertyName = new(statusPropertyNameKey);
            EnvironmentVariable taskDoneStatusName = new(taskDoneStatusNameKey);

            if (EnvironmentValidator.Validate([
                notionToken,
                taskDatabaseID,
                sprintDatabaseID,
                datePropertyName,
                namePropertyName,
                statusPropertyName,
                taskDoneStatusName]))
            {
                throw new ArgumentException("必要な環境変数が見つかりませんでした。");
            }

            return new NotionEnvironment(
                notionToken,
                taskDatabaseID,
                sprintDatabaseID,
                datePropertyName,
                namePropertyName,
                statusPropertyName,
                GetTaskDoneStatuses(taskDoneStatusName));
        }

        public readonly string NotionToken;
        public readonly string TaskDatabaseID;
        public readonly string SprintDatabaseID;
        public readonly string DatePropertyName;
        public readonly string NamePropertyName;
        public readonly string StatusPropertyName;
        public readonly string[] TaskDoneStatusName;

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine($"NotionToken: {(string.IsNullOrEmpty(NotionToken) ? "null or empty" : "set")}");
            sb.AppendLine($"TaskDatabaseID: {TaskDatabaseID}");
            sb.AppendLine($"SprintDatabaseID: {SprintDatabaseID}");
            sb.AppendLine($"DatePropertyName: {DatePropertyName}");
            sb.AppendLine($"NamePropertyName: {NamePropertyName}");
            sb.AppendLine($"StatusPropertyName: {StatusPropertyName}");
            sb.AppendLine($"TaskDoneStatusName: {TaskDoneStatusName}");
            return sb.ToString();
        }

        /// <summary>
        ///     カンマ区切りの完了ステータス名を配列に分割する。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string[] GetTaskDoneStatuses(string value)
        {
            return value.Split(',').Select(s => s.Trim()).ToArray();
        }
    }
}
