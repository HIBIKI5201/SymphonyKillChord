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
                CheckBotPermissions(_channelID);
                return;
            }

            await channel.SendMessageAsync(content);
            Console.WriteLine("メッセージ送信完了");
        }

        private readonly string _botToken;
        private readonly ulong _channelID;
        private readonly DiscordSocketClient _client;

        private void CheckBotPermissions(ulong channelId)
        {
            var channel = _client.GetChannel(channelId);

            // チャンネルがギルドチャンネルか判定
            if (channel is not SocketGuildChannel guildChannel)
            {
                Console.WriteLine("ギルドチャンネルではありません。");
                return;
            }

            // ギルド内の Bot 自身を取得
            var botUser = guildChannel.Guild.CurrentUser;

            // Bot の権限を取得
            var perms = botUser.GetPermissions(guildChannel);

            Console.WriteLine("=== Bot がこのチャンネルで持っている権限 ===");
            Console.WriteLine($"SendMessages: {perms.SendMessages}");
            Console.WriteLine($"ViewChannel:  {perms.ViewChannel}");
            Console.WriteLine($"EmbedLinks:   {perms.EmbedLinks}");
            Console.WriteLine($"AttachFiles:  {perms.AttachFiles}");
            Console.WriteLine($"MentionEveryone: {perms.MentionEveryone}");
        }
    }
}
