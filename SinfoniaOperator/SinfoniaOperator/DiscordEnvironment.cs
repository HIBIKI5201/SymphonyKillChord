using System;
using System.Text;

namespace SinfoniaStudio.SinfoniaOperator
{
    internal struct DiscordEnvironment
    {
        public DiscordEnvironment(
            string discordBotTokenKey,
            string discordChannelIDKey)
        {
            EnvironmentVariable discordBotToken = new(discordBotTokenKey);
            EnvironmentVariable discordChannelID = new(discordChannelIDKey);

            if (EnvironmentValidator.Validate([
                discordBotToken,
                discordChannelID]))
            {
                throw new ArgumentException("必要な環境変数が見つかりませんでした。");
            }

            DiscordBotToken = discordBotToken.Value;

            if (!ulong.TryParse(discordChannelID.Value, out DiscordChannelID))
            {
                Console.WriteLine($"DISCORD_CHANNEL_IDが数値に変換できませんでした。\nvalue {discordChannelID.Value}");
            }
        }

        public readonly string DiscordBotToken;
        public readonly ulong DiscordChannelID;

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine($"DiscordBotToken: {(string.IsNullOrEmpty(DiscordBotToken) ? "null or empty" : "set")}, ");
            sb.AppendLine($"DiscordChannelID: {DiscordChannelID}");
            return sb.ToString();
        }
    }
}
