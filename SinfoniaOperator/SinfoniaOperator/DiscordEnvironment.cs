using System;
using System.Text;

namespace SinfoniaStudio.SinfoniaOperator
{
    internal struct DiscordEnvironment
    {
        public DiscordEnvironment(
            string discordBotTokenKey,
            string discordTaskChannelIDKey,
            string discordSprintChannelIDKey)
        {
            EnvironmentVariable discordBotToken = new(discordBotTokenKey);
            EnvironmentVariable discordTaskChannelID = new(discordTaskChannelIDKey);
            EnvironmentVariable discordSprintChannelID = new(discordSprintChannelIDKey);

            if (EnvironmentValidator.Validate([
                discordBotToken,
                discordTaskChannelID,
                discordSprintChannelID]))
            {
                throw new ArgumentException("必要な環境変数が見つかりませんでした。");
            }

            DiscordBotToken = discordBotToken;
            DiscordTaskChannelID = discordTaskChannelID;
            DiscordSprintChannelID = discordSprintChannelID;
        }

        public readonly string DiscordBotToken;
        public readonly ulong DiscordTaskChannelID;
        public readonly ulong DiscordSprintChannelID;

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine($"DiscordBotToken: {(string.IsNullOrEmpty(DiscordBotToken) ? "null or empty" : "set")}, ");
            sb.AppendLine($"DiscordChannelID: {DiscordTaskChannelID}");
            sb.AppendLine($"DiscordSprintChannelID: {DiscordSprintChannelID}");
            return sb.ToString();
        }
    }
}
