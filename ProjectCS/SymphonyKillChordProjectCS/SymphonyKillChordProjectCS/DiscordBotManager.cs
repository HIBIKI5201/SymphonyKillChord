using System.Text;
using System.Text.Json;

namespace SinfoniaStudio.SinfoniaOperator
{
    internal class DiscordBotManager
    {
        public DiscordBotManager(string webhookUrl)
        {
            _webhookUrl = webhookUrl;
        }

        /// <summary>
        ///     タスクリストの文字列をDiscordに出力します。
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task PushTaskListAsync(string content)
        {
            using HttpClient client = new();

            var payload = new { content };
            string json = JsonSerializer.Serialize(payload);
            HttpResponseMessage response = await client.PostAsync(
                _webhookUrl,
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            Console.WriteLine($"Discord送信結果: {response.StatusCode}");
        }

        private readonly string _webhookUrl;
    }
}
