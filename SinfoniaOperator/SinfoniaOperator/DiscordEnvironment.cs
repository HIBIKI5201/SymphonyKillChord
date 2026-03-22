using System;
using System.Text;

namespace SinfoniaStudio.SinfoniaOperator
{
    internal struct DiscordEnvironment
    {
        public DiscordEnvironment(
            string discordBotTokenKey,
            string discordTaskChannelIDKey)
        {
            EnvironmentVariable discordBotToken = new(discordBotTokenKey);
            EnvironmentVariable discordTaskChannelID = new(discordTaskChannelIDKey);

            if (EnvironmentValidator.Validate([
                discordBotToken,
                discordTaskChannelID]))
            {
                throw new ArgumentException("必要な環境変数が見つかりませんでした。");
            }

            DiscordBotToken = discordBotToken;
            DiscordTaskChannelID = discordTaskChannelID;
        }

        public readonly string DiscordBotToken;
        public readonly ulong DiscordTaskChannelID;

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine($"DiscordBotToken: {(string.IsNullOrEmpty(DiscordBotToken) ? "null or empty" : "set")}, ");
            sb.AppendLine($"DiscordChannelID: {DiscordTaskChannelID}");
            return sb.ToString();
        }
    }
}
