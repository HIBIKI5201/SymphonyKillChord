using Discord;
using Discord.WebSocket;
using System.Threading.Channels;

namespace SinfoniaStudio.SinfoniaOperator
{
    internal class DiscordBotManager
    {
        public DiscordBotManager(string botToken, ulong channelID)
        {
            _botToken = botToken;
            _channelID = channelID;

            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All
            };
            _client = new DiscordSocketClient(config);
        }

        public async Task Awake()
        {
            await _client.LoginAsync(TokenType.Bot, _botToken);
            await _client.StartAsync();
        }

        /// <summary>
        ///     タスクリストの文字列をDiscordに出力します。
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task PushTaskListAsync(string content)
        {
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
    }
}
