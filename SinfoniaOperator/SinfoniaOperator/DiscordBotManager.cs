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
                _readyTcs.TrySetResult(default);
                return Task.CompletedTask;
            };

            try
            {
                await _client.LoginAsync(TokenType.Bot, _env.DiscordBotToken);
                await _client.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Discordボットの起動に失敗しました: {ex.Message}");
                throw;
            }

            await _readyTcs.Task;
        }

        /// <summary>
        ///     タスクリストの文字列をDiscordに出力します。
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task PushTaskChannelAsync(string content)
        {
            await _readyTcs.Task;

            ulong channelID = _env.DiscordTaskChannelID;
            await PushContextAsync(channelID, content);
            Console.WriteLine($" タスクチャンネルに文字を送信しました。");
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

            if (_client.GetChannel(channelID) is not IMessageChannel channel)
            {
                Console.WriteLine($"id:{channelID} のチャンネルがメッセージチャンネルにキャストできませんでした");
                return;
            }

            await channel.SendMessageAsync(content);
        }

        private struct Void { }
    }
}
