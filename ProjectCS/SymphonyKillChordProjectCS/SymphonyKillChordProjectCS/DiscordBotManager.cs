using Discord;
using Discord.WebSocket;

namespace SinfoniaStudio.SinfoniaOperator
{
    internal class DiscordBotManager
    {
        public DiscordBotManager(string botToken)
        {
            _botToken = botToken;
        }

        public async Task Awake()
        {
            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All
            };

            _client = new DiscordSocketClient(config);

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

        private DiscordSocketClient _client;
    }
}
