using System;
using System.Threading.Tasks;

namespace SinfoniaStudio.SinfoniaOperator
{
    internal static class SinfoniaOperator 
    {
        private const string DISCORD_BOT_TOKEN = "DISCORD_BOT_TOKEN";
        private const string DISCORD_CHANNEL_ID = "DISCORD_CHANNEL_ID";
        private const string NOTION_TOKEN = "NOTION_TOKEN";
        private const string NOTION_DATABASE_ID = "NOTION_DATABASE_ID";
        private const string NOTION_DATABASE_DATE_PROPERTY = "NOTION_DATABASE_DATE_PROPERTY";
        private const string NOTION_DATABASE_NAME_PROPERTY = "NOTION_DATABASE_NAME_PROPERTY";
        private const string NOTION_DATABASE_STATUS_PROPERTY = "NOTION_DATABASE_STATUS_PROPERTY";
        private const string NOTION_DATABASE_STATUS_TASK_DONE_PROPERTY = "NOTION_DATABASE_STATUS_TASK_DONE_PROPERTY";

        public static async Task Main()
        {
            DiscordEnvironment discordEnv = new DiscordEnvironment(
                DISCORD_BOT_TOKEN,
                DISCORD_CHANNEL_ID);
            NotionEnvironment notionEnv = new NotionEnvironment(
                NOTION_TOKEN,
                NOTION_DATABASE_ID,
                NOTION_DATABASE_DATE_PROPERTY,
                NOTION_DATABASE_NAME_PROPERTY,
                NOTION_DATABASE_STATUS_PROPERTY,
                NOTION_DATABASE_STATUS_TASK_DONE_PROPERTY);

            Console.WriteLine("環境変数のチェックが完了しました。\n" +
                $"discord env {discordEnv}\n" +
                $"notion env {notionEnv}");

            // ワーカークラスのインスタンスを生成。
            NotionTaskListReader taskReader = new(notionEnv);
            DiscordBotManager discordBot = new(discordEnv);

            // タスクを開始。
            Task<string> getTaskList = taskReader.GetTaskContent();
            Task discordAwake = discordBot.Awake();

            await Task.WhenAll(getTaskList, discordAwake);

            string content = getTaskList.Result;
            if (string.IsNullOrEmpty(content))
            {
                Console.WriteLine("タスクリストのコンテンツに何もないため終了");
                return;
            }

            await discordBot.PushTaskListAsync(content);
        }
    }
}