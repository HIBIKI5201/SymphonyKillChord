using System;
using System.Net;

namespace SinfoniaStudio.DiscordLogExporter
{
    /// <summary>
    ///     Discord APIがエラーレスポンスを返したことを表す例外。
    /// </summary>
    internal sealed class DiscordApiException : Exception
    {
        /// <summary>
        ///     HTTPステータスとレスポンス本文から例外を生成する。
        /// </summary>
        /// <param name="statusCode">HTTPステータス。</param>
        /// <param name="responseBody">Discord APIのレスポンス本文。</param>
        internal DiscordApiException(HttpStatusCode statusCode, string responseBody)
            : base($"Discord APIがエラーを返しました（{(int)statusCode} {statusCode}）: {responseBody}")
        {
        }
    }
}
