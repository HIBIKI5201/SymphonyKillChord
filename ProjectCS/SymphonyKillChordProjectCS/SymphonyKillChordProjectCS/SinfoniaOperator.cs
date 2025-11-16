namespace SinfoniaStudio.SinfoniaOperator
{
    internal static class SinfoniaOperator 
    {
        private const string DISCORD_BOT_TOKEN = "DISCORD_BOT_TOKEN";
        private const string NOTION_TOKEN = "NOTION_TOKEN";
        private const string NOTION_DATABASE_ID = "NOTION_DATABASE_ID";
        private const string NOTION_DATABASE_DATE_PROPERTY = "NOTION_DATABASE_DATE_PROPERTY";
        private const string NOTION_DATABASE_NAME_PROPERTY = "NOTION_DATABASE_NAME_PROPERTY";

        static async Task Main()
        {
            // GitHubから環境変数を取得。
            string discordBotToken = Environment.GetEnvironmentVariable(DISCORD_BOT_TOKEN) ?? string.Empty;
            string notionToken = Environment.GetEnvironmentVariable(NOTION_TOKEN) ?? string.Empty;
            string databaseID = Environment.GetEnvironmentVariable(NOTION_DATABASE_ID) ?? string.Empty;
            string datePropertyName = Environment.GetEnvironmentVariable(NOTION_DATABASE_DATE_PROPERTY) ?? string.Empty;
            string namePropertyName = Environment.GetEnvironmentVariable(NOTION_DATABASE_NAME_PROPERTY) ?? string.Empty;

            // バリデーションチェックを行い、nullが一つでもあれば終了する。
            if (ValidateEnvironmentVariable(
                discordBotToken,
                notionToken,
                databaseID,
                datePropertyName,
                namePropertyName))
            {
                return;
            }

            NotionTaskListReader taskReader = new(
                notionToken,
                databaseID,
                datePropertyName,
                namePropertyName);
            DiscordBotManager discordBot = new(discordBotToken);

            string content = await taskReader.GetTaskContent();
            if (string.IsNullOrEmpty(content))
            {
                Console.WriteLine("タスクリストのコンテンツに何もないため終了");
                return; 
            }
            await discordBot.PushTaskListAsync(content);
        }


        /// <summary>
        ///     環境変数のバリデーションチェック。
        /// </summary>
        /// 
        /// <param name="notionToken"></param>
        /// <param name="databaseID"></param>
        /// <param name="datePropertyName"></param>
        /// <param name="namePropertyName"></param>
        /// <returns>チェックで失敗があったかどうか</returns>
        private static bool ValidateEnvironmentVariable(
            string discordBotAPIKey,
            string notionToken,
            string databaseID,
            string datePropertyName,
            string namePropertyName)
        {
            bool isNullOrWhiteSpace = false;

            if (string.IsNullOrEmpty(discordBotAPIKey))
            {
                Console.WriteLine("環境変数 DISCORD_BOT_TOKEN が設定されていません。");
                isNullOrWhiteSpace = true;
            }
            if (string.IsNullOrEmpty(notionToken))
            {
                Console.WriteLine("環境変数 NOTION_TOKEN が設定されていません。");
                isNullOrWhiteSpace = true;
            }
            if (string.IsNullOrEmpty(databaseID))
            {
                Console.WriteLine("環境変数 NOTION_DATABASE_ID が設定されていません。");
                isNullOrWhiteSpace = true;
            }
            if (string.IsNullOrEmpty(datePropertyName))
            {
                Console.WriteLine("環境変数 NOTION_DATABASE_DATE_PROPERTY が設定されていません。");
                isNullOrWhiteSpace = true;
            }
            if (string.IsNullOrEmpty(namePropertyName))
            {
                Console.WriteLine("環境変数 NOTION_DATABASE_NAME_PROPERTY が設定されていません。");
                isNullOrWhiteSpace = true;
            }

            return isNullOrWhiteSpace;
        }
    }
}