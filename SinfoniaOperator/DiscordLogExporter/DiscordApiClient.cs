using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace SinfoniaStudio.DiscordLogExporter
{
    /// <summary>
    ///     Discord REST APIからチャンネル、スレッド、メッセージを取得するクラス。
    /// </summary>
    internal sealed class DiscordApiClient : IDisposable
    {
        private const string API_BASE_URL = "https://discord.com/api/v10/";
        private const int PAGE_SIZE = 100;
        private const int MAX_RATE_LIMIT_RETRIES = 8;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;
        private bool _isDisposed;

        /// <summary>
        ///     Botトークンを用いるDiscord APIクライアントを生成する。
        /// </summary>
        /// <param name="botToken">Discord Botトークン。</param>
        internal DiscordApiClient(string botToken)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(API_BASE_URL),
                Timeout = TimeSpan.FromMinutes(5)
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", botToken);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SinfoniaOperator-DiscordLogExporter/1.0");
        }

        /// <summary>
        ///     指定IDのチャンネル情報を取得する。
        /// </summary>
        /// <param name="channelId">チャンネルID。</param>
        /// <returns>チャンネル情報。</returns>
        internal Task<DiscordChannel> GetChannelAsync(ulong channelId)
        {
            return GetAsync<DiscordChannel>($"channels/{channelId}");
        }

        /// <summary>
        ///     チャンネルの全メッセージを取得する。
        /// </summary>
        /// <param name="channelId">チャンネルまたはスレッドID。</param>
        /// <returns>投稿日時の古い順に並んだメッセージ一覧。</returns>
        internal async Task<IReadOnlyList<DiscordMessage>> GetMessagesAsync(ulong channelId)
        {
            List<DiscordMessage> messages = new();
            string? before = null;
            while (true)
            {
                string path = $"channels/{channelId}/messages?limit={PAGE_SIZE}";
                if (before != null)
                {
                    path += $"&before={Uri.EscapeDataString(before)}";
                }

                List<DiscordMessage> page = await GetAsync<List<DiscordMessage>>(path);
                messages.AddRange(page);
                if (page.Count < PAGE_SIZE) { break; }

                before = page[^1].Id;
            }

            return messages
                .GroupBy(message => message.Id, StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(message => message.Timestamp)
                .ThenBy(message => ParseSnowflake(message.Id))
                .ToList();
        }

        /// <summary>
        ///     フォーラムに属するアクティブ・アーカイブ済みの全投稿スレッドを取得する。
        /// </summary>
        /// <param name="forumChannel">フォーラムチャンネル。</param>
        /// <returns>作成日時の古い順に並んだ投稿スレッド一覧。</returns>
        internal async Task<IReadOnlyList<DiscordChannel>> GetForumThreadsAsync(DiscordChannel forumChannel)
        {
            if (string.IsNullOrWhiteSpace(forumChannel.GuildId))
            {
                throw new InvalidOperationException($"フォーラム {forumChannel.Id} のサーバーIDを取得できませんでした。");
            }

            DiscordThreadListResponse activeResponse = await GetAsync<DiscordThreadListResponse>(
                $"guilds/{forumChannel.GuildId}/threads/active");
            List<DiscordChannel> threads = activeResponse.Threads
                .Where(thread => string.Equals(thread.ParentId, forumChannel.Id, StringComparison.Ordinal))
                .ToList();

            string? before = null;
            while (true)
            {
                string path = $"channels/{forumChannel.Id}/threads/archived/public?limit={PAGE_SIZE}";
                if (before != null)
                {
                    path += $"&before={Uri.EscapeDataString(before)}";
                }

                DiscordThreadListResponse archivedResponse = await GetAsync<DiscordThreadListResponse>(path);
                threads.AddRange(archivedResponse.Threads);
                if (!archivedResponse.HasMore || archivedResponse.Threads.Count == 0) { break; }

                DiscordChannel lastThread = archivedResponse.Threads[^1];
                if (lastThread.ThreadMetadata == null)
                {
                    throw new InvalidOperationException("アーカイブ済みスレッドのページ位置を取得できませんでした。");
                }

                before = lastThread.ThreadMetadata.ArchiveTimestamp.ToUniversalTime().ToString("O");
            }

            return threads
                .GroupBy(thread => thread.Id, StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(thread => ParseSnowflake(thread.Id))
                .ToList();
        }

        /// <summary>
        ///     HTTPクライアントを解放する。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) { return; }

            _httpClient.Dispose();
            _isDisposed = true;
        }

        /// <summary>
        ///     Discord APIへGET要求を送り、JSONレスポンスを指定型へ変換する。
        ///     レート制限時はDiscordが返す待機時間に従って再試行する。
        /// </summary>
        /// <typeparam name="T">レスポンスの型。</typeparam>
        /// <param name="relativePath">APIベースURLからの相対パス。</param>
        /// <returns>変換済みレスポンス。</returns>
        private async Task<T> GetAsync<T>(string relativePath)
        {
            for (int retryCount = 0; retryCount <= MAX_RATE_LIMIT_RETRIES; retryCount++)
            {
                using HttpResponseMessage response = await _httpClient.GetAsync(relativePath);
                string responseBody = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.TooManyRequests && retryCount < MAX_RATE_LIMIT_RETRIES)
                {
                    DiscordRateLimitResponse? rateLimit = JsonSerializer.Deserialize<DiscordRateLimitResponse>(
                        responseBody,
                        _jsonOptions);
                    double seconds = Math.Max(rateLimit?.RetryAfterSeconds ?? 1.0, 0.1);
                    await Task.Delay(TimeSpan.FromSeconds(seconds));
                    continue;
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw new DiscordApiException(response.StatusCode, responseBody);
                }

                T? result = JsonSerializer.Deserialize<T>(responseBody, _jsonOptions);
                if (result == null)
                {
                    throw new InvalidOperationException("Discord APIのレスポンスを読み取れませんでした。");
                }

                return result;
            }

            throw new InvalidOperationException("Discord APIのレート制限により、再試行回数を超過しました。");
        }

        /// <summary>
        ///     DiscordのSnowflake文字列を並び替え用の数値へ変換する。
        /// </summary>
        /// <param name="value">Snowflake文字列。</param>
        /// <returns>変換できた場合はSnowflake、それ以外は0。</returns>
        private static ulong ParseSnowflake(string value)
        {
            return ulong.TryParse(value, out ulong result) ? result : 0;
        }
    }
}
