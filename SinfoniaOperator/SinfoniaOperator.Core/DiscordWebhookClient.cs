using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SinfoniaStudio.SinfoniaOperator
{
    /// <summary>
    ///     DiscordのWebhookへメッセージを送信するクラス。
    ///     Botのゲートウェイ接続を必要としないため、Unityエディタ等からも利用できる。
    /// </summary>
    public class DiscordWebhookClient
    {
        public DiscordWebhookClient(string webhookUrl)
        {
            _webhookUrl = webhookUrl;
        }

        /// <summary>
        ///     Webhookへメッセージを送信する。
        ///     2000文字を超える場合は改行位置で分割して送信する。
        /// </summary>
        /// <param name="content"></param>
        /// <returns>全ての送信に成功した場合はtrue。</returns>
        public async Task<bool> SendMessageAsync(string content)
        {
            if (string.IsNullOrWhiteSpace(_webhookUrl))
            {
                OperatorLog.Write("[DiscordWebhook] WebhookのURLが設定されていないため、送信をスキップしました。");
                return false;
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                OperatorLog.Write("[DiscordWebhook] 送信内容が空のため、送信をスキップしました。");
                return false;
            }

            foreach (string chunk in DiscordMessageUtility.SplitContent(content))
            {
                if (!await PostChunkAsync(chunk))
                {
                    return false;
                }
            }

            OperatorLog.Write("[DiscordWebhook] メッセージ送信が完了しました。");
            return true;
        }

        private static readonly HttpClient _http = new();

        private readonly string _webhookUrl;

        /// <summary>
        ///     1チャンク分をWebhookへPOSTする。
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        private async Task<bool> PostChunkAsync(string chunk)
        {
            try
            {
                var payload = new Dictionary<string, string>
                {
                    { "content", chunk }
                };
                string jsonBody = JsonConvert.SerializeObject(payload);
                using StringContent body = new(jsonBody, Encoding.UTF8, "application/json");

                HttpResponseMessage resp = await _http.PostAsync(_webhookUrl, body);
                if (!resp.IsSuccessStatusCode)
                {
                    string errorBody = await resp.Content.ReadAsStringAsync();
                    OperatorLog.Write($"[DiscordWebhook] 送信に失敗しました: {resp.StatusCode} - {errorBody}");
                    return false;
                }

                return true;
            }
            catch (System.Exception ex)
            {
                OperatorLog.Write($"[DiscordWebhook] 送信中にエラーが発生しました: {ex.Message}");
                return false;
            }
        }
    }
}
