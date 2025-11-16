using Discord;
using Discord.WebSocket;

namespace SinfoniaStudio.SinfoniaOperator
{
    internal class DiscordBotManager
    {
        public DiscordBotManager(string botToken, int channelID)
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

        }

        private readonly string _botToken;
        private readonly int _channelID;
        private readonly DiscordSocketClient _client;
    }
}
