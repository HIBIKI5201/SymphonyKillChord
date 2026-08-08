using System;
using System.Threading.Tasks;

namespace SinfoniaStudio.SinfoniaOperator
{
    /// <summary>
    ///     論理チャンネル名(DiscordChannelKind)を指定するだけでDiscordへメッセージを送信するクラス。
    ///     チャンネルIDの解決とBot/Webhookの送信手段選択を一箇所に集約し、
    ///     呼び出し側(Unityエディタ・Bot本体CLI等)がそれぞれ重複実装しないようにする。
    /// </summary>
    public static class DiscordNotifier
    {
        /// <summary>
        ///     指定した論理チャンネルへメッセージを送信する。
        ///     Botトークンが設定されていればボットアカウントとして送信し、なければWebhookで送信する。
        /// </summary>
        /// <param name="channel">送信先の論理チャンネル。</param>
        /// <param name="content">送信内容。</param>
        /// <returns>送信に成功した場合はtrue。</returns>
        public static async Task<bool> SendAsync(DiscordChannelKind channel, string content)
        {
            string botToken = OperatorConfig.GetValue(OperatorConfigKeys.DISCORD_BOT_TOKEN);
            string webhookUrl = OperatorConfig.GetValue(OperatorConfigKeys.DISCORD_WEBHOOK_URL);

            if (!string.IsNullOrWhiteSpace(botToken))
            {
                ulong channelId = ResolveChannelId(channel);
                DiscordBotRestClient client = new(botToken);
                return await client.SendMessageAsync(channelId, content);
            }

            if (!string.IsNullOrWhiteSpace(webhookUrl))
            {
                DiscordWebhookClient client = new(webhookUrl);
                return await client.SendMessageAsync(content);
            }

            OperatorLog.Write($"[DiscordNotifier] {OperatorConfigKeys.DISCORD_BOT_TOKEN} / {OperatorConfigKeys.DISCORD_WEBHOOK_URL} が未設定のため送信をスキップしました。");
            return false;
        }

        /// <summary>
        ///     論理チャンネルに対応する設定キーから、実際のDiscordチャンネルIDを解決する。
        /// </summary>
        /// <param name="channel">送信先の論理チャンネル。</param>
        /// <returns>解決したチャンネルID。</returns>
        private static ulong ResolveChannelId(DiscordChannelKind channel)
        {
            string key = channel switch
            {
                DiscordChannelKind.Task => OperatorConfigKeys.DISCORD_TASK_CHANNEL_ID,
                DiscordChannelKind.TaskAlert => OperatorConfigKeys.DISCORD_TASK_ALERT_CHANNEL_ID,
                DiscordChannelKind.Sprint => OperatorConfigKeys.DISCORD_SPRINT_CHANNEL_ID,
                DiscordChannelKind.WorkLog => OperatorConfigKeys.DISCORD_WORK_LOG_CHANNEL_ID,
                _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, null),
            };

            string raw = OperatorConfig.GetValue(key);
            if (!ulong.TryParse(raw, out ulong channelId))
            {
                throw new InvalidOperationException($"設定値 {key} が未設定か、数値として解釈できませんでした。");
            }

            return channelId;
        }
    }
}
