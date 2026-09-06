using Discord;
using Discord.WebSocket;
using SinfoniaStudio.SinfoniaOperator.SpecSearch;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SinfoniaStudio.SinfoniaOperator
{
    /// <summary>
    ///     Discord Botの接続、通知送信、仕様検索コマンドを管理する。
    /// </summary>
    internal sealed class DiscordBotManager : IAsyncDisposable
    {
        /// <summary>
        ///     既存の通知用Discord設定でBotを生成する。
        /// </summary>
        /// <param name="env">Discordの通知設定。</param>
        public DiscordBotManager(DiscordEnvironment env)
            : this(env.DiscordBotToken)
        {
            _discordTaskChannelId = env.DiscordTaskChannelID;
            _discordTaskAlertChannelId = env.DiscordTaskAlertChannelID;
            _discordSprintChannelId = env.DiscordSprintChannelID;
        }

        /// <summary>
        ///     Botトークンだけを使用してDiscord Botを生成する。
        /// </summary>
        /// <param name="discordBotToken">Discord Botのトークン。</param>
        public DiscordBotManager(string discordBotToken)
        {
            if (string.IsNullOrWhiteSpace(discordBotToken))
            {
                throw new ArgumentException("Discord Botトークンを指定してください。", nameof(discordBotToken));
            }

            _discordBotToken = discordBotToken;
            DiscordSocketConfig config = new()
            {
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.DirectMessages
            };
            _client = new DiscordSocketClient(config);
        }

        /// <summary> Botの準備が完了するまで待機するタスク。 </summary>
        public Task AwakeTask => _readySource.Task;

        /// <summary>
        ///     仕様検索コマンドに必要な依存情報を設定する。
        /// </summary>
        /// <param name="specIndex">読み込み済みの仕様検索インデックス。</param>
        /// <param name="embeddingModel">クエリ用の埋め込みモデル。</param>
        /// <param name="guildId">コマンドを限定登録する任意のGuild ID。</param>
        /// <param name="topK">返却する検索結果の件数。</param>
        public void ConfigureSpecSearch(SpecIndex specIndex, IEmbeddingModel embeddingModel, ulong? guildId, int topK)
        {
            _specIndex = specIndex ?? throw new ArgumentNullException(nameof(specIndex));
            _embeddingModel = embeddingModel ?? throw new ArgumentNullException(nameof(embeddingModel));
            _specSearchGuildId = guildId;
            _specSearchTopK = topK;
        }

        /// <summary>
        ///     Discordへログインし、Gateway接続の準備完了まで待機する。
        /// </summary>
        public async Task Awake()
        {
            _client.Ready += ReadyHandler;
            _client.Log += LogHandler;
            _client.InteractionCreated += InteractionCreatedHandler;

            try
            {
                Console.WriteLine("[DiscordBot] ログインを開始します...");
                await _client.LoginAsync(TokenType.Bot, _discordBotToken);
                await _client.StartAsync();
                await _readySource.Task;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DiscordBot] Discordボットの起動に失敗しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        ///     タスクチャンネルへ文字列を送信する。
        /// </summary>
        /// <param name="content">送信する本文。</param>
        public async Task PushTaskChannelAsync(string content)
        {
            await _readySource.Task;
            Console.WriteLine($"[DiscordBot] タスクチャンネルへの送信を試みます (ChannelID: {_discordTaskChannelId})");
            await PushContextAsync(_discordTaskChannelId, content);
        }

        /// <summary>
        ///     タスクアラートチャンネルへ文字列を送信する。
        /// </summary>
        /// <param name="content">送信する本文。</param>
        public async Task PushTaskAlertChannelAsync(string content)
        {
            await _readySource.Task;
            Console.WriteLine($"[DiscordBot] タスクアラートチャンネルへの送信を試みます (ChannelID: {_discordTaskAlertChannelId})");
            await PushContextAsync(_discordTaskAlertChannelId, content);
        }

        /// <summary>
        ///     スプリントチャンネルへ文字列を送信する。
        /// </summary>
        /// <param name="content">送信する本文。</param>
        public async Task PushSprintChannelAsync(string content)
        {
            await _readySource.Task;
            Console.WriteLine($"[DiscordBot] スプリントチャンネルへの送信を試みます (ChannelID: {_discordSprintChannelId})");
            await PushContextAsync(_discordSprintChannelId, content);
        }

        /// <summary>
        ///     Discord接続と関連リソースを非同期に破棄する。
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            _client.Ready -= ReadyHandler;
            _client.Log -= LogHandler;
            _client.InteractionCreated -= InteractionCreatedHandler;
            await _client.StopAsync();
            await _client.LogoutAsync();
            _client.Dispose();
        }

        private const int MAX_MESSAGE_LENGTH = 2000;
        private const int EXCERPT_LENGTH = 400;
        private const int MAX_FIELD_NAME_LENGTH = 256;
        private const int MAX_QUERY_DISPLAY_LENGTH = 500;
        private const string SPEC_COMMAND_NAME = "spec";
        private const string QUERY_OPTION_NAME = "query";
        private const string QUERY_PREFIX = "query: ";
        private const string OMITTED_MARK = "…";

        private readonly string _discordBotToken;
        private readonly DiscordSocketClient _client;
        private readonly TaskCompletionSource _readySource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private ulong _discordTaskChannelId;
        private ulong _discordTaskAlertChannelId;
        private ulong _discordSprintChannelId;
        private SpecIndex? _specIndex;
        private IEmbeddingModel? _embeddingModel;
        private ulong? _specSearchGuildId;
        private int _specSearchTopK;
        private bool _isSpecCommandRegistered;

        /// <summary>
        ///     Discord接続の準備完了を記録し、仕様検索コマンドを登録する。
        /// </summary>
        private async Task ReadyHandler()
        {
            Console.WriteLine("[DiscordBot] ボットが準備完了しました。");
            try
            {
                if (_specIndex != null && !_isSpecCommandRegistered)
                {
                    await RegisterSpecCommandAsync();
                    _isSpecCommandRegistered = true;
                }

                _readySource.TrySetResult();
            }
            catch (Exception ex)
            {
                _readySource.TrySetException(ex);
                Console.WriteLine($"[DiscordBot] 仕様検索コマンドの登録に失敗しました: {ex.Message}");
            }
        }

        /// <summary>
        ///     Discordクライアントのログを標準出力へ転送する。
        /// </summary>
        /// <param name="log">Discordのログメッセージ。</param>
        private static Task LogHandler(LogMessage log)
        {
            Console.WriteLine($"[DiscordBot Log] {log}");
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Discord Interactionを仕様検索処理へ振り分ける。
        /// </summary>
        /// <param name="interaction">受信したInteraction。</param>
        private async Task InteractionCreatedHandler(SocketInteraction interaction)
        {
            if (interaction is not SocketSlashCommand command ||
                !string.Equals(command.Data.Name, SPEC_COMMAND_NAME, StringComparison.Ordinal))
            {
                return;
            }

            await command.DeferAsync();
            try
            {
                SocketSlashCommandDataOption? queryOption = command.Data.Options
                    .FirstOrDefault(option => string.Equals(option.Name, QUERY_OPTION_NAME, StringComparison.Ordinal));
                string query = queryOption?.Value as string ?? string.Empty;
                if (string.IsNullOrWhiteSpace(query))
                {
                    await command.FollowupAsync("検索文字列を入力してください。");
                    return;
                }

                SpecIndex index = _specIndex ?? throw new InvalidOperationException("仕様検索インデックスが設定されていません。");
                IEmbeddingModel embeddingModel = _embeddingModel ?? throw new InvalidOperationException("埋め込みモデルが設定されていません。");
                float[] queryVector = await embeddingModel.EmbedAsync(QUERY_PREFIX + query);
                SpecChunkRecord[] records = index.TopK(queryVector, _specSearchTopK);
                EmbedBuilder embedBuilder = BuildSearchResultEmbed(query, records);
                await command.FollowupAsync(embeds: [embedBuilder.Build()]);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DiscordBot] 仕様検索に失敗しました: {ex.Message}");
                await command.FollowupAsync("仕様検索中にエラーが発生しました。");
            }
        }

        /// <summary>
        ///     Discordへ仕様検索スラッシュコマンドを登録する。
        /// </summary>
        private async Task RegisterSpecCommandAsync()
        {
            SlashCommandBuilder commandBuilder = new SlashCommandBuilder()
                .WithName(SPEC_COMMAND_NAME)
                .WithDescription("仕様書から関連する記述を検索します。")
                .AddOption(
                    QUERY_OPTION_NAME,
                    ApplicationCommandOptionType.String,
                    "検索する語句や質問",
                    isRequired: true);

            ApplicationCommandProperties command = commandBuilder.Build();
            if (_specSearchGuildId.HasValue)
            {
                await _client.Rest.CreateGuildCommand(command, _specSearchGuildId.Value);
                Console.WriteLine($"[DiscordBot] /specをGuild {_specSearchGuildId.Value} に登録しました。");
            }
            else
            {
                await _client.CreateGlobalApplicationCommandAsync(command);
                Console.WriteLine("[DiscordBot] /specをグローバルコマンドとして登録しました。");
            }
        }

        /// <summary>
        ///     検索結果をDiscord Embedへ変換する。
        /// </summary>
        /// <param name="query">ユーザーが入力した検索文字列。</param>
        /// <param name="records">類似度順の仕様書チャンク。</param>
        /// <returns>検索結果のEmbed構築器。</returns>
        private static EmbedBuilder BuildSearchResultEmbed(string query, SpecChunkRecord[] records)
        {
            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle("仕様検索結果")
                .WithDescription($"検索: {Truncate(query, MAX_QUERY_DISPLAY_LENGTH)}")
                .WithColor(Color.Blue);

            if (records.Length == 0)
            {
                builder.WithDescription($"検索: {Truncate(query, MAX_QUERY_DISPLAY_LENGTH)}\n該当する仕様が見つかりませんでした。");
                return builder;
            }

            foreach (SpecChunkRecord record in records)
            {
                string fieldName = Truncate(record.HeadingBreadcrumb, MAX_FIELD_NAME_LENGTH);
                string excerpt = Truncate(record.Text, EXCERPT_LENGTH);
                string notionLink = string.IsNullOrWhiteSpace(record.NotionUrl)
                    ? "Notionリンクなし"
                    : $"[Notionで開く]({record.NotionUrl})";
                builder.AddField(fieldName, $"{excerpt}\n\n{notionLink}");
            }

            return builder;
        }

        /// <summary>
        ///     文字列を指定した最大文字数へ省略する。
        /// </summary>
        /// <param name="value">対象文字列。</param>
        /// <param name="maximumLength">最大文字数。</param>
        /// <returns>必要に応じて省略した文字列。</returns>
        private static string Truncate(string value, int maximumLength)
        {
            if (value.Length <= maximumLength)
            {
                return value;
            }

            return value[..(maximumLength - OMITTED_MARK.Length)] + OMITTED_MARK;
        }

        /// <summary>
        ///     指定されたDiscordチャンネルへ文字列を分割送信する。
        /// </summary>
        /// <param name="channelId">送信先チャンネルID。</param>
        /// <param name="content">送信する本文。</param>
        private async Task PushContextAsync(ulong channelId, string content)
        {
            await _readySource.Task;
            if (string.IsNullOrWhiteSpace(content))
            {
                Console.WriteLine($"[DiscordBot] id:{channelId} への送信内容が空のため、送信をスキップしました。");
                return;
            }

            if (_client.GetChannel(channelId) is not IMessageChannel channel)
            {
                Console.WriteLine($"[DiscordBot] id:{channelId} のチャンネルが見つからないか、メッセージチャンネルではありません。");
                return;
            }

            string remainingContent = content;
            try
            {
                while (remainingContent.Length > MAX_MESSAGE_LENGTH)
                {
                    int splitIndex = remainingContent.LastIndexOf('\n', MAX_MESSAGE_LENGTH - 1);
                    if (splitIndex == -1)
                    {
                        splitIndex = MAX_MESSAGE_LENGTH;
                    }
                    else
                    {
                        splitIndex++;
                    }

                    string chunk = remainingContent[..splitIndex];
                    await channel.SendMessageAsync(chunk);
                    remainingContent = remainingContent[splitIndex..];
                }

                if (!string.IsNullOrWhiteSpace(remainingContent))
                {
                    await channel.SendMessageAsync(remainingContent);
                }

                Console.WriteLine($"[DiscordBot] id:{channelId} へのメッセージ送信が完了しました。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DiscordBot] メッセージ送信中にエラーが発生しました: {ex.Message}");
            }
        }
    }
}
