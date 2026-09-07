using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SinfoniaStudio.SinfoniaOperator.SpecSearch;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SinfoniaStudio.SinfoniaOperator
{
    /// <summary>
    ///     Gemini APIを使用して仕様検索結果を要約する。
    /// </summary>
    public sealed class GeminiSummarizer : IDisposable
    {
        /// <summary>
        ///     Gemini APIの接続情報を使用して要約器を生成する。
        /// </summary>
        /// <param name="apiKey">Gemini APIキー。</param>
        /// <param name="model">使用するGeminiモデル名。</param>
        public GeminiSummarizer(string apiKey, string model)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
            ArgumentException.ThrowIfNullOrWhiteSpace(model);

            _apiKey = apiKey;
            _model = model;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(HTTP_TIMEOUT_SECONDS)
            };
        }

        /// <summary>
        ///     仕様書チャンクだけを根拠に、ユーザーの質問への回答を生成する。
        /// </summary>
        /// <param name="query">ユーザーの質問文。</param>
        /// <param name="records">要約の根拠にする仕様書チャンク。</param>
        /// <param name="cancellationToken">処理を中止するトークン。</param>
        /// <returns>Geminiが生成した日本語の要約文。</returns>
        public async Task<string> SummarizeAsync(
            string query,
            IReadOnlyList<SpecChunkRecord> records,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentNullException.ThrowIfNull(records);

            string prompt = BuildPrompt(query, records);
            string requestJson = JsonConvert.SerializeObject(new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            });
            string endpoint = string.Format(
                GEMINI_API_ENDPOINT_FORMAT,
                Uri.EscapeDataString(_model),
                Uri.EscapeDataString(_apiKey));
            using StringContent content = new(requestJson, Encoding.UTF8, JSON_MEDIA_TYPE);
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException("Gemini APIへのリクエストに失敗しました。", ex);
            }

            using HttpResponseMessage responseScope = response;
            if (!responseScope.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Gemini APIがHTTPステータス {(int)responseScope.StatusCode} ({responseScope.StatusCode}) を返しました。");
            }

            string responseJson = await responseScope.Content.ReadAsStringAsync(cancellationToken);
            JObject responseObject = JObject.Parse(responseJson);
            JToken? candidate = responseObject["candidates"]?.First;
            string? finishReason = candidate?["finishReason"]?.Value<string>();
            if (!string.Equals(finishReason, NORMAL_FINISH_REASON, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Gemini APIの生成が正常終了しませんでした。終了理由: {finishReason ?? "不明"}");
            }

            string? summary = candidate?["content"]?["parts"]?.First?["text"]?.Value<string>();
            if (string.IsNullOrWhiteSpace(summary))
            {
                throw new InvalidOperationException("Gemini APIのレスポンスから要約文を取得できませんでした。");
            }

            return summary;
        }

        /// <summary>
        ///     内部で使用するHTTPクライアントを破棄する。
        /// </summary>
        public void Dispose()
        {
            _httpClient.Dispose();
        }

        private const int HTTP_TIMEOUT_SECONDS = 15;
        private const string GEMINI_API_ENDPOINT_FORMAT = "https://generativelanguage.googleapis.com/v1beta/models/{0}:generateContent?key={1}";
        private const string JSON_MEDIA_TYPE = "application/json";
        private const string NORMAL_FINISH_REASON = "STOP";
        private const string PROMPT_INSTRUCTION = "以下の仕様書の抜粋だけを根拠に、質問に日本語で簡潔に答えてください。抜粋に無い情報は答えないでください。";
        private readonly string _apiKey;
        private readonly string _model;
        private readonly HttpClient _httpClient;

        /// <summary>
        ///     質問文と仕様書チャンクからGeminiへ渡すプロンプトを構築する。
        /// </summary>
        /// <param name="query">ユーザーの質問文。</param>
        /// <param name="records">要約の根拠にする仕様書チャンク。</param>
        /// <returns>Geminiへ渡すプロンプト。</returns>
        private static string BuildPrompt(string query, IReadOnlyList<SpecChunkRecord> records)
        {
            StringBuilder builder = new();
            builder.AppendLine(PROMPT_INSTRUCTION);
            builder.AppendLine();
            builder.AppendLine("仕様書の抜粋:");
            foreach (SpecChunkRecord record in records)
            {
                builder.Append('[').Append(record.HeadingBreadcrumb).AppendLine("]");
                builder.AppendLine(record.Text);
                builder.AppendLine();
            }

            builder.AppendLine("質問:");
            builder.Append(query);
            return builder.ToString();
        }
    }
}
