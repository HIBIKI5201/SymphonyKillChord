using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SinfoniaStudio.SinfoniaOperator
{
    /// <summary>
    ///     DiscordのREST APIを使い、ボットアカウントとしてメッセージを送信するクラス。
    ///     ゲートウェイ（WebSocket常駐）接続を必要としないため、Unityエディタ等からも利用できる。
    /// </summary>
    public class DiscordBotRestClient
    {
        public DiscordBotRestClient(string botToken)
        {
            _botToken = botToken;
        }

        /// <summary>
        ///     指定したチャンネルへボットとしてメッセージを送信する。
        ///     2000文字を超える場合は改行位置で分割して送信する。
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="content"></param>
        /// <returns>全ての送信に成功した場合はtrue。</returns>
        public async Task<bool> SendMessageAsync(ulong channelId, string content)
        {
            if (string.IsNullOrWhiteSpace(_botToken))
            {
                OperatorLog.Write("[DiscordBotRest] Botトークンが設定されていないため、送信をスキップしました。");
                return false;
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                OperatorLog.Write("[DiscordBotRest] 送信内容が空のため、送信をスキップしました。");
                return false;
            }

            foreach (string chunk in DiscordMessageUtility.SplitContent(content))
            {
                if (!await PostChunkAsync(channelId, chunk))
                {
                    return false;
                }
            }

            OperatorLog.Write($"[DiscordBotRest] id:{channelId} へのメッセージ送信が完了しました。");
            return true;
        }

        private const string API_BASE_URL = "https://discord.com/api/v10";

        private static readonly HttpClient _http = new();

        private readonly string _botToken;

        /// <summary>
        ///     1チャンク分をチャンネルへPOSTする。
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="chunk"></param>
        /// <returns></returns>
        private async Task<bool> PostChunkAsync(ulong channelId, string chunk)
        {
            try
            {
                var payload = new Dictionary<string, string>
                {
                    { "content", chunk }
                };
                string jsonBody = JsonConvert.SerializeObject(payload);

                using HttpRequestMessage request = new(HttpMethod.Post, $"{API_BASE_URL}/channels/{channelId}/messages");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bot", _botToken);
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                HttpResponseMessage resp = await _http.SendAsync(request);
                if (!resp.IsSuccessStatusCode)
                {
                    string errorBody = await resp.Content.ReadAsStringAsync();
                    OperatorLog.Write($"[DiscordBotRest] 送信に失敗しました: {resp.StatusCode} - {errorBody}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                OperatorLog.Write($"[DiscordBotRest] 送信中にエラーが発生しました: {ex.Message}");
                return false;
            }
        }
    }
}
