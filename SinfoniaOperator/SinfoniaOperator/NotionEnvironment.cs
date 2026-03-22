using System;
using System.Text;

namespace SinfoniaStudio.SinfoniaOperator
{
    internal readonly struct NotionEnvironment
    {
        public NotionEnvironment(
            string notionTokenKey,
            string taskDatabaseIDKey,
            string datePropertyNameKey,
            string namePropertyNameKey,
            string statusPropertyNameKey,
            string taskDoneStatusNameKey)
        {
            EnvironmentVariable notionToken = new(notionTokenKey);
            EnvironmentVariable taskDatabaseID = new(taskDatabaseIDKey);
            EnvironmentVariable datePropertyName = new(datePropertyNameKey);
            EnvironmentVariable namePropertyName = new(namePropertyNameKey);
            EnvironmentVariable statusPropertyName = new(statusPropertyNameKey);
            EnvironmentVariable taskDoneStatusName = new(taskDoneStatusNameKey);

            if (EnvironmentValidator.Validate([
                notionToken,
                taskDatabaseID,
                datePropertyName,
                namePropertyName,
                statusPropertyName,
                taskDoneStatusName]))
            {
                throw new ArgumentException("必要な環境変数が見つかりませんでした。");
            }

            NotionToken = notionToken;
            TaskDatabaseID = taskDatabaseID;
            DatePropertyName = datePropertyName;
            NamePropertyName = namePropertyName;
            StatusPropertyName = statusPropertyName;
            TaskDoneStatusName = taskDoneStatusName;
        }

        public readonly string NotionToken;
        public readonly string TaskDatabaseID;
        public readonly string DatePropertyName;
        public readonly string NamePropertyName;
        public readonly string StatusPropertyName;
        public readonly string TaskDoneStatusName;

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine($"NotionToken: {(string.IsNullOrEmpty(NotionToken) ? "null or empty" : "set")}");
            sb.AppendLine($"DatabaseID: {TaskDatabaseID}");
            sb.AppendLine($"DatePropertyName: {DatePropertyName}");
            sb.AppendLine($"NamePropertyName: {NamePropertyName}");
            sb.AppendLine($"StatusPropertyName: {StatusPropertyName}");
            sb.AppendLine($"TaskDoneStatusName: {TaskDoneStatusName}");
            return sb.ToString();
        }
    }
}
