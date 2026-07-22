using System;
using System.Net;

namespace SinfoniaStudio.NotionMarkdownExporter
{
    /// <summary>
    ///     Notion APIの失敗内容を保持する例外。
    /// </summary>
    internal sealed class NotionApiException : Exception
    {
        /// <summary>
        ///     Notion API例外を生成する。
        /// </summary>
        /// <param name="statusCode">HTTPステータスコード。</param>
        /// <param name="message">エラーメッセージ。</param>
        internal NotionApiException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }

        internal HttpStatusCode StatusCode { get; }
    }
}
