using System;
using System.Threading.Tasks;

namespace SinfoniaStudio.SinfoniaOperator
{
    internal static class SinfoniaOperator
    {
        private const string DISCORD_BOT_TOKEN = "DISCORD_BOT_TOKEN";
        private const string DISCORD_TASK_CHANNEL_ID = "DISCORD_TASK_CHANNEL_ID";
        private const string NOTION_TOKEN = "NOTION_TOKEN";
        private const string NOTION_DATABASE_ID = "NOTION_DATABASE_ID";
        private const string NOTION_DATABASE_DATE_PROPERTY = "NOTION_DATABASE_DATE_PROPERTY";
        private const string NOTION_DATABASE_NAME_PROPERTY = "NOTION_DATABASE_NAME_PROPERTY";
        private const string NOTION_DATABASE_STATUS_PROPERTY = "NOTION_DATABASE_STATUS_PROPERTY";
        private const string NOTION_DATABASE_STATUS_TASK_DONE_PROPERTY = "NOTION_DATABASE_STATUS_TASK_DONE_PROPERTY";

        public static async Task Main()
        {
            DiscordEnvironment discordEnv = default;
            NotionEnvironment notionEnv = default;
            try
            {
                discordEnv = new DiscordEnvironment(
                    DISCORD_BOT_TOKEN,
                    DISCORD_TASK_CHANNEL_ID);
                notionEnv = new NotionEnvironment(
                    NOTION_TOKEN,
                    NOTION_DATABASE_ID,
                    NOTION_DATABASE_DATE_PROPERTY,
                    NOTION_DATABASE_NAME_PROPERTY,
                    NOTION_DATABASE_STATUS_PROPERTY,
                    NOTION_DATABASE_STATUS_TASK_DONE_PROPERTY);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return;
            }
            Console.WriteLine("環境変数のチェックが完了しました。\n" +
                $"discord env {discordEnv}\n" +
                $"notion env {notionEnv}");

            // ワーカークラスのインスタンスを生成。
            NotionTaskListReader taskReader = new(notionEnv);
            DiscordBotManager discordBot = new(discordEnv);

            // タスク取得を開始。
            Task<string> getTaskList = taskReader.GetTaskContent();
            Task discordAwake = discordBot.Awake();

            await Task.WhenAll(getTaskList, discordAwake);

            string content = getTaskList.Result;
            if (!string.IsNullOrEmpty(content))
            {
                await discordBot.PushTaskChannelAsync(content);
            }
            else
            {
                Console.WriteLine("タスクの内容が空なのでスキップ。");
            }


        }
    }
}