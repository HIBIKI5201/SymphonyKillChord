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
            _botToken = env.DiscordBotToken;
            _channelID = env.DiscordChannelID;

            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.DirectMessages
            };
            _client = new DiscordSocketClient(config);
        }

        public async Task Awake()
        {
            _client.Ready += () =>
            {
                _readyTcs.TrySetResult(default);
                return Task.CompletedTask;
            };

            try
            {
                await _client.LoginAsync(TokenType.Bot, _botToken);
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
        public async Task PushTaskListAsync(string content)
        {
            await _readyTcs.Task;

            if (_client.GetChannel(_channelID) is not IMessageChannel channel)
            {
                Console.WriteLine($"id:{_channelID} のチャンネルがメッセージチャンネルにキャストできませんでした");
                return;
            }

            await channel.SendMessageAsync(content);
            Console.WriteLine("メッセージ送信完了");
        }

        private readonly string _botToken;
        private readonly ulong _channelID;
        private readonly DiscordSocketClient _client;
        private readonly TaskCompletionSource<Void> _readyTcs = new();

        private struct Void { }
    }
}
