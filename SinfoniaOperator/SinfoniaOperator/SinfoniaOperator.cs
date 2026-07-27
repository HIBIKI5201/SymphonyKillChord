using System;
using System.IO;
using System.Threading.Tasks;

namespace SinfoniaStudio.SinfoniaOperator
{
    internal static class SinfoniaOperator
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("[Main] SinfoniaOperator 起動中...");

            // 設定ソースを選択する。JSON設定があれば優先し、なければ環境変数を使用する。
            if (!TryLoadConfig(args)) { return; }

            DiscordEnvironment discordEnv = default;
            NotionEnvironment notionEnv = default;
            try
            {
                discordEnv = new DiscordEnvironment(
                    OperatorConfigKeys.DISCORD_BOT_TOKEN,
                    OperatorConfigKeys.DISCORD_TASK_CHANNEL_ID,
                    OperatorConfigKeys.DISCORD_TASK_ALERT_CHANNEL_ID,
                    OperatorConfigKeys.DISCORD_SPRINT_CHANNEL_ID);
                notionEnv = NotionEnvironment.FromConfig(
                    OperatorConfigKeys.NOTION_TOKEN,
                    OperatorConfigKeys.NOTION_TASK_DATABASE_ID,
                    OperatorConfigKeys.NOTION_SPRINT_DATABASE_ID,
                    OperatorConfigKeys.NOTION_DATABASE_DATE_PROPERTY,
                    OperatorConfigKeys.NOTION_DATABASE_NAME_PROPERTY,
                    OperatorConfigKeys.NOTION_DATABASE_STATUS_PROPERTY,
                    OperatorConfigKeys.NOTION_DATABASE_STATUS_TASK_DONE_PROPERTY);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Main] 環境変数の読み込み中にエラーが発生しました: {ex.Message}");
                return;
            }

            Console.WriteLine("[Main] 環境変数のチェックが完了しました。");
            Console.WriteLine($"[Main] Notion設定 - 日付プロパティ名: '{notionEnv.DatePropertyName}', 名前プロパティ名: '{notionEnv.NamePropertyName}'");
            Console.WriteLine($"[Main] Discord設定 - タスクチャンネルID: '{discordEnv.DiscordTaskChannelID}', スプリントチャンネルID: '{discordEnv.DiscordSprintChannelID}', タスクアラートチャンネルID: '{discordEnv.DiscordTaskAlertChannelID}'");

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

        /// <summary>
        ///     JSON設定ファイルの読み込みを試みる。
        ///     引数でパスが指定された場合は、指定された全ファイルを順に読み込む。
        ///     指定が無い場合は公開設定、秘密設定の順に読み込み、
        ///     見つからない値は環境変数から取得する。
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static bool TryLoadConfig(string[] args)
        {
            OperatorConfig.ClearOverrides();

            if (args.Length > 0)
            {
                foreach (string path in args)
                {
                    if (!OperatorConfig.LoadJsonFile(path))
                    {
                        Console.WriteLine($"[Main] 指定されたJSON設定ファイルが見つかりません: {path}");
                        return false;
                    }
                }

                return true;
            }

            string? environmentConfigPath = FindConfigFile(OperatorConfig.ENVIRONMENT_CONFIG_FILE_NAME);
            string? secretsConfigPath = FindConfigFile(OperatorConfig.SECRETS_CONFIG_FILE_NAME);
            string? legacyConfigPath = FindConfigFile(OperatorConfig.LEGACY_CONFIG_FILE_NAME);

            if (environmentConfigPath != null)
            {
                OperatorConfig.LoadJsonFile(environmentConfigPath);
            }

            if (secretsConfigPath != null)
            {
                OperatorConfig.LoadJsonFile(secretsConfigPath);
            }
            else if (legacyConfigPath != null)
            {
                if (environmentConfigPath == null)
                {
                    OperatorConfig.LoadJsonFile(legacyConfigPath);
                }
                else
                {
                    OperatorConfig.LoadJsonFile(
                        legacyConfigPath,
                        OperatorConfigKeys.DISCORD_BOT_TOKEN,
                        OperatorConfigKeys.NOTION_TOKEN);
                }

                Console.WriteLine($"[Main] 旧設定ファイルを読み込みました。{OperatorConfig.SECRETS_CONFIG_FILE_NAME}への移行を推奨します。");
            }

            if (environmentConfigPath == null && secretsConfigPath == null && legacyConfigPath == null)
            {
                Console.WriteLine("[Main] JSON設定が見つからないため、環境変数を使用します。");
            }

            return true;
        }

        /// <summary>
        ///     カレントディレクトリと実行ファイル位置の祖先から設定ファイルを探す。
        /// </summary>
        /// <param name="fileName">設定ファイル名。</param>
        /// <returns>見つかったファイルのパス。見つからない場合はnull。</returns>
        private static string? FindConfigFile(string fileName)
        {
            string[] startDirectories =
            {
                Directory.GetCurrentDirectory(),
                AppContext.BaseDirectory
            };

            foreach (string startDirectory in startDirectories)
            {
                DirectoryInfo? current = new(Path.GetFullPath(startDirectory));
                while (current != null)
                {
                    string candidate = Path.Combine(current.FullName, fileName);
                    if (File.Exists(candidate)) { return candidate; }
                    current = current.Parent;
                }
            }

            return null;
        }

        private static async Task PushTaskList(NotionTaskListReader reader, DiscordBotManager discordBot)
        {
            if (DateTimeUtility.IsTodayByDayOfWeek(DayOfWeek.Sunday))
            {
                Console.WriteLine("[PushTaskList] 日曜日はタスク表を通知しません。");
                return;
            }

            Console.WriteLine("[PushTaskList] タスクリストの取得を開始します...");
            NotionTaskListReader.TaskContentResult taskContent = await reader.GetTaskContent();

            Task[] tasks = new Task[2];
            if (taskContent.HasTaskContent)
            {
                tasks[0] = discordBot.PushTaskChannelAsync(taskContent.TaskContent);
            }
            else
            {
                tasks[0] = Task.CompletedTask;
                Console.WriteLine("[PushTaskList] 送信するタスクがありませんでした。");
            }

            if (taskContent.HasTaskAlertContent)
            {
                tasks[1] = discordBot.PushTaskAlertChannelAsync(taskContent.TaskAlertContent);
            }
            else
            {
                tasks[1] = Task.CompletedTask;
                Console.WriteLine("[PushTaskList] 送信するタスクアラートがありませんでした。");
            }

            await Task.WhenAll(tasks);
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
