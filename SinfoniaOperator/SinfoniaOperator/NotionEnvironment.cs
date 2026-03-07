using System;
using System.Text;

namespace SinfoniaStudio.SinfoniaOperator
{
    internal readonly struct NotionEnvironment
    {
        public NotionEnvironment(
            string notionTokenKey,
            string databaseIDKey,
            string datePropertyNameKey,
            string namePropertyNameKey,
            string statusPropertyNameKey,
            string taskDoneStatusNameKey)
        {
            EnvironmentVariable notionToken = new(notionTokenKey);
            EnvironmentVariable databaseID = new(databaseIDKey);
            EnvironmentVariable datePropertyName = new(datePropertyNameKey);
            EnvironmentVariable namePropertyName = new(namePropertyNameKey);
            EnvironmentVariable statusPropertyName = new(statusPropertyNameKey);
            EnvironmentVariable taskDoneStatusName = new(taskDoneStatusNameKey);

            if (EnvironmentValidator.Validate([
                notionToken,
                databaseID,
                datePropertyName,
                namePropertyName,
                statusPropertyName,
                taskDoneStatusName]))
            {
                throw new ArgumentException("必要な環境変数が見つかりませんでした。");
            }

            NotionToken = notionToken.Value;
            DatabaseID = databaseID.Value;
            DatePropertyName = datePropertyName.Value;
            NamePropertyName = namePropertyName.Value;
            StatusPropertyName = statusPropertyName.Value;
            TaskDoneStatusName = taskDoneStatusName.Value;
        }

        public readonly string NotionToken;
        public readonly string DatabaseID;
        public readonly string DatePropertyName;
        public readonly string NamePropertyName;
        public readonly string StatusPropertyName;
        public readonly string TaskDoneStatusName;

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine($"NotionToken: {(string.IsNullOrEmpty(NotionToken) ? "null or empty" : "set")}");
            sb.AppendLine($"DatabaseID: {DatabaseID}");
            sb.AppendLine($"DatePropertyName: {DatePropertyName}");
            sb.AppendLine($"NamePropertyName: {NamePropertyName}");
            sb.AppendLine($"StatusPropertyName: {StatusPropertyName}");
            sb.AppendLine($"TaskDoneStatusName: {TaskDoneStatusName}");
            return sb.ToString();
        }
    }
}
