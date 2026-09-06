using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SinfoniaStudio.SinfoniaOperator.SpecSearch;

namespace SinfoniaStudio.SinfoniaOperator
{
    internal static class SinfoniaOperator
    {
        /// <summary>
        ///     指定されたサブコマンドまたは既存の定期通知処理を実行する。
        /// </summary>
        /// <param name="args">コマンドライン引数。</param>
        public static async Task Main(string[] args)
        {
            if (args.Length > 0 && string.Equals(args[0], "send", StringComparison.OrdinalIgnoreCase))
            {
                await RunSendCommandAsync(args[1..]);
                return;
            }

            if (args.Length > 0 && string.Equals(args[0], "index", StringComparison.OrdinalIgnoreCase))
            {
                await RunIndexCommandAsync(args[1..]);
                return;
            }

            if (args.Length > 0 && string.Equals(args[0], "serve", StringComparison.OrdinalIgnoreCase))
            {
                await RunServeCommandAsync(args[1..]);
                return;
            }

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

        private const int DEFAULT_TOP_K = 3;
        private const int MAXIMUM_TOP_K = 10;
        private const string TOKENIZER_FILE_NAME = "sentencepiece.bpe.model";

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

            LoadConfigFromDefaultLocations();
            return true;
        }

        /// <summary>
        ///     カレントディレクトリと実行ファイル位置の祖先から公開・秘密JSON設定を探して読み込む。
        ///     見つからない値は環境変数から取得する。
        ///     `send`サブコマンドなど、引数をJSON設定パスとして扱わない呼び出し元からも利用する。
        /// </summary>
        private static void LoadConfigFromDefaultLocations()
        {
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

        /// <summary>
        ///     自動ビルド等の外部プロセスから、Notionを読み込まずにDiscordへ1メッセージだけ送る軽量コマンド。
        ///     使用法: send --channel &lt;Task|TaskAlert|Sprint|WorkLog&gt; --message &lt;本文&gt;
        /// </summary>
        /// <param name="args">"send"を除いた残りの引数。</param>
        private static async Task RunSendCommandAsync(string[] args)
        {
            string? channelArg = null;
            string? message = null;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--channel" when i + 1 < args.Length:
                        channelArg = args[++i];
                        break;
                    case "--message" when i + 1 < args.Length:
                        message = args[++i];
                        break;
                }
            }

            if (string.IsNullOrWhiteSpace(channelArg) || string.IsNullOrWhiteSpace(message))
            {
                Console.WriteLine("[Send] 使用法: send --channel <Task|TaskAlert|Sprint|WorkLog> --message <本文>");
                Environment.ExitCode = 1;
                return;
            }

            if (!Enum.TryParse(channelArg, ignoreCase: true, out DiscordChannelKind channel))
            {
                string validValues = string.Join(", ", Enum.GetNames(typeof(DiscordChannelKind)));
                Console.WriteLine($"[Send] チャンネル '{channelArg}' は無効です。有効な値: {validValues}");
                Environment.ExitCode = 1;
                return;
            }

            OperatorConfig.ClearOverrides();
            LoadConfigFromDefaultLocations();

            try
            {
                bool isSucceeded = await DiscordNotifier.SendAsync(channel, message);
                if (isSucceeded)
                {
                    Console.WriteLine($"[Send] {channel} チャンネルへの送信が完了しました。");
                }
                else
                {
                    Console.WriteLine($"[Send] {channel} チャンネルへの送信に失敗しました。");
                    Environment.ExitCode = 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Send] 送信中にエラーが発生しました: {ex.Message}");
                Environment.ExitCode = 1;
            }
        }

        /// <summary>
        ///     仕様書を埋め込み、検索インデックスを生成する。
        /// </summary>
        /// <param name="args">"index"を除いたJSON設定ファイルのパス。</param>
        private static async Task RunIndexCommandAsync(string[] args)
        {
            if (!TryLoadConfig(args))
            {
                Environment.ExitCode = 1;
                return;
            }

            try
            {
                string indexPath = GetRequiredConfigValue(OperatorConfigKeys.SPEC_SEARCH_INDEX_PATH);
                string modelPath = GetRequiredConfigValue(OperatorConfigKeys.SPEC_SEARCH_EMBEDDING_MODEL_PATH);
                string tokenizerPath = GetTokenizerPath(modelPath);
                string repositoryRoot = FindRepositoryRoot();
                MarkdownChunker chunker = new(repositoryRoot);
                using OnnxEmbeddingModel embeddingModel = new(modelPath, tokenizerPath);
                SpecIndexBuilder indexBuilder = new(chunker, embeddingModel);
                SpecIndex index = await indexBuilder.BuildAndSaveAsync(indexPath);
                Console.WriteLine($"[SpecSearch] インデックスを生成しました: {indexPath} ({index.Records.Count} 件)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SpecSearch] インデックス生成に失敗しました: {ex.Message}");
                Environment.ExitCode = 1;
            }
        }

        /// <summary>
        ///     Discordの仕様検索Botを起動し、終了シグナルまで待機する。
        /// </summary>
        /// <param name="args">"serve"を除いたJSON設定ファイルのパス。</param>
        private static async Task RunServeCommandAsync(string[] args)
        {
            if (!TryLoadConfig(args))
            {
                Environment.ExitCode = 1;
                return;
            }

            try
            {
                string discordBotToken = GetRequiredConfigValue(OperatorConfigKeys.DISCORD_BOT_TOKEN);
                string indexPath = GetRequiredConfigValue(OperatorConfigKeys.SPEC_SEARCH_INDEX_PATH);
                string modelPath = GetRequiredConfigValue(OperatorConfigKeys.SPEC_SEARCH_EMBEDDING_MODEL_PATH);
                string tokenizerPath = GetTokenizerPath(modelPath);
                ulong? guildId = ParseOptionalGuildId(OperatorConfig.GetValue(OperatorConfigKeys.SPEC_SEARCH_DISCORD_GUILD_ID));
                int topK = ParseTopK(OperatorConfig.GetValue(OperatorConfigKeys.SPEC_SEARCH_TOP_K));
                SpecIndex index = SpecIndex.Load(indexPath);
                using OnnxEmbeddingModel embeddingModel = new(modelPath, tokenizerPath);
                await using DiscordBotManager discordBot = new(discordBotToken);
                discordBot.ConfigureSpecSearch(index, embeddingModel, guildId, topK);

                TaskCompletionSource shutdownSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
                using PosixSignalRegistration interruptRegistration = PosixSignalRegistration.Create(
                    PosixSignal.SIGINT,
                    context =>
                    {
                        context.Cancel = true;
                        shutdownSource.TrySetResult();
                    });
                using PosixSignalRegistration terminateRegistration = PosixSignalRegistration.Create(
                    PosixSignal.SIGTERM,
                    context =>
                    {
                        context.Cancel = true;
                        shutdownSource.TrySetResult();
                    });

                await discordBot.Awake();
                Console.WriteLine("[SpecSearch] 仕様検索Botを起動しました。終了シグナルを待機します。");
                await shutdownSource.Task;
                Console.WriteLine("[SpecSearch] 終了シグナルを受信しました。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SpecSearch] Botの実行に失敗しました: {ex.Message}");
                Environment.ExitCode = 1;
            }
        }

        /// <summary>
        ///     必須の設定値を取得する。
        /// </summary>
        /// <param name="key">設定キー。</param>
        /// <returns>空でない設定値。</returns>
        private static string GetRequiredConfigValue(string key)
        {
            string value = OperatorConfig.GetValue(key);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"必須設定 {key} が指定されていません。");
            }

            return value;
        }

        /// <summary>
        ///     ONNXモデルと同じディレクトリにあるトークナイザのパスを取得する。
        /// </summary>
        /// <param name="modelPath">ONNXモデルファイルのパス。</param>
        /// <returns>SentencePieceモデルファイルのパス。</returns>
        private static string GetTokenizerPath(string modelPath)
        {
            string? directoryPath = Path.GetDirectoryName(Path.GetFullPath(modelPath));
            return Path.Combine(directoryPath ?? Directory.GetCurrentDirectory(), TOKENIZER_FILE_NAME);
        }

        /// <summary>
        ///     仕様書ディレクトリを含むリポジトリルートを探索する。
        /// </summary>
        /// <returns>リポジトリルートの絶対パス。</returns>
        private static string FindRepositoryRoot()
        {
            string[] startDirectories = [Directory.GetCurrentDirectory(), AppContext.BaseDirectory];
            foreach (string startDirectory in startDirectories)
            {
                DirectoryInfo? directory = new(Path.GetFullPath(startDirectory));
                while (directory != null)
                {
                    string specificationPath = Path.Combine(directory.FullName, "Docs", "NotionSpecifications");
                    if (Directory.Exists(specificationPath))
                    {
                        return directory.FullName;
                    }

                    directory = directory.Parent;
                }
            }

            throw new DirectoryNotFoundException("Docs/NotionSpecificationsを含むリポジトリルートが見つかりません。");
        }

        /// <summary>
        ///     任意のDiscord Guild IDを解析する。
        /// </summary>
        /// <param name="value">設定された文字列。</param>
        /// <returns>設定されたGuild ID。未設定の場合はnull。</returns>
        private static ulong? ParseOptionalGuildId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (!ulong.TryParse(value, out ulong guildId))
            {
                throw new FormatException($"{OperatorConfigKeys.SPEC_SEARCH_DISCORD_GUILD_ID} は符号なし整数で指定してください。");
            }

            return guildId;
        }

        /// <summary>
        ///     仕様検索の取得件数を解析する。
        /// </summary>
        /// <param name="value">設定された文字列。</param>
        /// <returns>許容範囲に収めた取得件数。</returns>
        private static int ParseTopK(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return DEFAULT_TOP_K;
            }

            if (!int.TryParse(value, out int topK) || topK <= 0 || topK > MAXIMUM_TOP_K)
            {
                throw new FormatException($"{OperatorConfigKeys.SPEC_SEARCH_TOP_K} は1から{MAXIMUM_TOP_K}の整数で指定してください。");
            }

            return topK;
        }

        /// <summary>
        ///     今日のタスクとタスクアラートをDiscordへ送信する。
        /// </summary>
        /// <param name="reader">タスクの読み取り元。</param>
        /// <param name="discordBot">Discord Bot。</param>
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

        /// <summary>
        ///     月曜日にスプリント情報をDiscordへ送信する。
        /// </summary>
        /// <param name="reader">スプリントの読み取り元。</param>
        /// <param name="discordBot">Discord Bot。</param>
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
