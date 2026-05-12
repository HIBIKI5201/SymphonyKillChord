using System;
using System.Threading.Tasks;

namespace SinfoniaStudio.SinfoniaOperator
{
    internal static class SinfoniaOperator
    {
        private const string DISCORD_BOT_TOKEN = "DISCORD_BOT_TOKEN";
        private const string DISCORD_TASK_CHANNEL_ID = "DISCORD_TASK_CHANNEL_ID";
        private const string DISCORD_SPRINT_CHANNEL_ID = "DISCORD_SPRINT_CHANNEL_ID";
        private const string NOTION_TOKEN = "NOTION_TOKEN";
        private const string NOTION_TASK_DATABASE_ID = "NOTION_TASK_DATABASE_ID";
        private const string NOTION_SPRINT_DATABASE_ID = "NOTION_SPRINT_DATABASE_ID";
        private const string NOTION_DATABASE_DATE_PROPERTY = "NOTION_DATABASE_DATE_PROPERTY";
        private const string NOTION_DATABASE_NAME_PROPERTY = "NOTION_DATABASE_NAME_PROPERTY";
        private const string NOTION_DATABASE_STATUS_PROPERTY = "NOTION_DATABASE_STATUS_PROPERTY";
        private const string NOTION_DATABASE_STATUS_TASK_DONE_PROPERTY = "NOTION_DATABASE_STATUS_TASK_DONE_PROPERTY";

        public static async Task Main()
        {
            Console.WriteLine("[Main] SinfoniaOperator 起動中...");
            DiscordEnvironment discordEnv = default;
            NotionEnvironment notionEnv = default;
            try
            {
                discordEnv = new DiscordEnvironment(
                    DISCORD_BOT_TOKEN,
                    DISCORD_TASK_CHANNEL_ID,
                    DISCORD_SPRINT_CHANNEL_ID);
                notionEnv = new NotionEnvironment(
                    NOTION_TOKEN,
                    NOTION_TASK_DATABASE_ID,
                    NOTION_SPRINT_DATABASE_ID,
                    NOTION_DATABASE_DATE_PROPERTY,
                    NOTION_DATABASE_NAME_PROPERTY,
                    NOTION_DATABASE_STATUS_PROPERTY,
                    NOTION_DATABASE_STATUS_TASK_DONE_PROPERTY);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Main] 環境変数の読み込み中にエラーが発生しました: {ex.Message}");
                return;
            }

            Console.WriteLine("[Main] 環境変数のチェックが完了しました。");
            Console.WriteLine($"[Main] Notion設定 - 日付プロパティ名: '{notionEnv.DatePropertyName}', 名前プロパティ名: '{notionEnv.NamePropertyName}'");

            // ワーカークラスのインスタンスを生成。
            NotionTaskListReader taskReader = new(notionEnv);
            NotionSprintListReader sprintReader = new(notionEnv);
            DiscordBotManager discordBot = new(discordEnv);

            // タスク取得を開始。
            Console.WriteLine("[Main] Discordボットの初期化を開始します...");
            await discordBot.Awake();

            Console.WriteLine("[Main] 各リーダーによる情報の取得を開始します...");
            Task taskListTask = PushTaskList(taskReader, discordBot);
            Task sprintTask = PushSprint(sprintReader, discordBot);

            await Task.WhenAll(taskListTask, sprintTask);
            Console.WriteLine("[Main] 全ての処理が完了しました。");
        }

        private static async Task PushTaskList(NotionTaskListReader reader, DiscordBotManager discordBot)
        {
            if (DateTimeUtility.IsTodayByDayOfWeek(DayOfWeek.Sunday))
            {
                Console.WriteLine("[PushTaskList] 日曜日はタスク表を通知しません。");
                return;
            }

            Console.WriteLine("[PushTaskList] タスクリストの取得を開始します...");
            string taskContent = await reader.GetTaskContent();
            if (string.IsNullOrEmpty(taskContent))
            {
                Console.WriteLine("[PushTaskList] 送信するタスクがありませんでした。");
                return;
            }

            await discordBot.PushTaskChannelAsync(taskContent);
        }

        private static async Task PushSprint(NotionSprintListReader reader, DiscordBotManager discordBot)
        {
            await discordBot.AwakeTask;

            DayOfWeek targetDay = DayOfWeek.Monday;
            if (!DateTimeUtility.IsTodayByDayOfWeek(targetDay))
            {
                Console.WriteLine($"[PushSprint] 今日は {targetDay} ではないため、スプリントの処理をスキップします。 (今日は {DateTimeUtility.JstNow().DayOfWeek})");
                return;
            }

            Console.WriteLine($"[PushSprint] 今日は {targetDay} なので、スプリントの内容も取得します。");
            string sprintContent = await reader.GetSprintContent();
            if (string.IsNullOrEmpty(sprintContent))
            {
                Console.WriteLine("[PushSprint] 送信するスプリント情報がありませんでした。");
                return;
            }

            await discordBot.PushSprintChannelAsync(sprintContent);
        }
    }
}