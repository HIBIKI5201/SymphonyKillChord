using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace SinfoniaStudio.SinfoniaOperator
{
    internal class DiscordBotManager
    {
        public DiscordBotManager(DiscordEnvironment env)
        {
            _env = env;
            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.DirectMessages
            };
            _client = new DiscordSocketClient(config);
        }

        public Task AwakeTask => _readyTcs.Task;

        public async Task Awake()
        {
            _client.Ready += () =>
            {
                Console.WriteLine("[DiscordBot] ボットが準備完了しました。");
                _readyTcs.TrySetResult(default);
                return Task.CompletedTask;
            };

            _client.Log += (log) =>
            {
                Console.WriteLine($"[DiscordBot Log] {log.Message}");
                return Task.CompletedTask;
            };

            try
            {
                Console.WriteLine("[DiscordBot] ログインを開始します...");
                await _client.LoginAsync(TokenType.Bot, _env.DiscordBotToken);
                await _client.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DiscordBot] Discordボットの起動に失敗しました: {ex.Message}");
                throw;
            }

            await _readyTcs.Task;
        }

        /// <summary>
        ///     タスクチャンネルに文字列をDiscordに出力します。
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task PushTaskChannelAsync(string content)
        {
            await _readyTcs.Task;

            ulong channelID = _env.DiscordTaskChannelID;
            Console.WriteLine($"[DiscordBot] タスクチャンネルへの送信を試みます (ChannelID: {channelID})");
            await PushContextAsync(channelID, content);
        }

        /// <summary>
        ///     スプリントチャンネルに文字列をDiscordに出力します。
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task PushSprintChannelAsync(string content)
        {
            await _readyTcs.Task;

            ulong channelID = _env.DiscordSprintChannelID;
            Console.WriteLine($"[DiscordBot] スプリントチャンネルへの送信を試みます (ChannelID: {channelID})");
            await PushContextAsync(channelID, content);
        }


        private readonly DiscordEnvironment _env;
        private readonly DiscordSocketClient _client;
        private readonly TaskCompletionSource<Void> _readyTcs = new();

        /// <summary>
        ///     入力されたチャンネルに文字を送信します。
        /// </summary>
        /// <param name="channelID"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private async Task PushContextAsync(ulong channelID, string content)
        {
            await _readyTcs.Task;

            if (string.IsNullOrWhiteSpace(content))
            {
                Console.WriteLine($"[DiscordBot] id:{channelID} への送信内容が空のため、送信をスキップしました。");
                return;
            }

            if (_client.GetChannel(channelID) is not IMessageChannel channel)
            {
                Console.WriteLine($"[DiscordBot] id:{channelID} のチャンネルが見つからないか、メッセージチャンネルではありません。");
                return;
            }

            const int MAX_LENGTH = 2000;
            string remainingContent = content;

            try
            {
                while (remainingContent.Length > MAX_LENGTH)
                {
                    // 2000文字以内で最後の改行を探す
                    int splitIndex = remainingContent.LastIndexOf('\n', MAX_LENGTH - 1);

                    // 改行が見つからない場合は2000文字で強制分割
                    if (splitIndex == -1)
                    {
                        splitIndex = MAX_LENGTH;
                    }
                    else
                    {
                        // 改行文字そのものを含めて分割する
                        splitIndex++;
                    }

                    string chunk = remainingContent.Substring(0, splitIndex);
                    await channel.SendMessageAsync(chunk);
                    remainingContent = remainingContent.Substring(splitIndex);
                }

                if (!string.IsNullOrWhiteSpace(remainingContent))
                {
                    await channel.SendMessageAsync(remainingContent);
                }

                Console.WriteLine($"[DiscordBot] id:{channelID} へのメッセージ送信が完了しました。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DiscordBot] メッセージ送信中にエラーが発生しました: {ex.Message}");
            }
        }

        private struct Void { }
    }
}
