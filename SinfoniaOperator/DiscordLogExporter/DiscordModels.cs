using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SinfoniaStudio.DiscordLogExporter
{
    /// <summary>
    ///     Discord APIのチャンネル情報を保持するクラス。
    /// </summary>
    internal sealed class DiscordChannel
    {
        [JsonInclude, JsonPropertyName("id")]
        internal string Id { get; set; } = string.Empty;

        [JsonInclude, JsonPropertyName("type")]
        internal int Type { get; set; }

        [JsonInclude, JsonPropertyName("guild_id")]
        internal string? GuildId { get; set; }

        [JsonInclude, JsonPropertyName("parent_id")]
        internal string? ParentId { get; set; }

        [JsonInclude, JsonPropertyName("name")]
        internal string Name { get; set; } = string.Empty;

        [JsonInclude, JsonPropertyName("thread_metadata")]
        internal DiscordThreadMetadata? ThreadMetadata { get; set; }
    }

    /// <summary>
    ///     Discord APIのスレッド状態を保持するクラス。
    /// </summary>
    internal sealed class DiscordThreadMetadata
    {
        [JsonInclude, JsonPropertyName("archive_timestamp")]
        internal DateTimeOffset ArchiveTimestamp { get; set; }
    }

    /// <summary>
    ///     Discord APIのメッセージ情報を保持するクラス。
    /// </summary>
    internal sealed class DiscordMessage
    {
        [JsonInclude, JsonPropertyName("id")]
        internal string Id { get; set; } = string.Empty;

        [JsonInclude, JsonPropertyName("content")]
        internal string Content { get; set; } = string.Empty;

        [JsonInclude, JsonPropertyName("timestamp")]
        internal DateTimeOffset Timestamp { get; set; }

        [JsonInclude, JsonPropertyName("edited_timestamp")]
        internal DateTimeOffset? EditedTimestamp { get; set; }

        [JsonInclude, JsonPropertyName("author")]
        internal DiscordUser Author { get; set; } = new();

        [JsonInclude, JsonPropertyName("attachments")]
        internal List<DiscordAttachment> Attachments { get; set; } = new();

        [JsonInclude, JsonPropertyName("embeds")]
        internal List<DiscordEmbed> Embeds { get; set; } = new();

        [JsonInclude, JsonPropertyName("sticker_items")]
        internal List<DiscordSticker> Stickers { get; set; } = new();
    }

    /// <summary>
    ///     Discord APIのユーザー情報を保持するクラス。
    /// </summary>
    internal sealed class DiscordUser
    {
        [JsonInclude, JsonPropertyName("username")]
        internal string Username { get; set; } = string.Empty;

        [JsonInclude, JsonPropertyName("global_name")]
        internal string? GlobalName { get; set; }

        [JsonInclude, JsonPropertyName("discriminator")]
        internal string Discriminator { get; set; } = "0";

        [JsonInclude, JsonPropertyName("bot")]
        internal bool IsBot { get; set; }
    }

    /// <summary>
    ///     Discord APIの添付ファイル情報を保持するクラス。
    /// </summary>
    internal sealed class DiscordAttachment
    {
        [JsonInclude, JsonPropertyName("filename")]
        internal string FileName { get; set; } = string.Empty;

        [JsonInclude, JsonPropertyName("url")]
        internal string Url { get; set; } = string.Empty;
    }

    /// <summary>
    ///     Discord APIの埋め込み情報を保持するクラス。
    /// </summary>
    internal sealed class DiscordEmbed
    {
        [JsonInclude, JsonPropertyName("title")]
        internal string? Title { get; set; }

        [JsonInclude, JsonPropertyName("description")]
        internal string? Description { get; set; }

        [JsonInclude, JsonPropertyName("url")]
        internal string? Url { get; set; }
    }

    /// <summary>
    ///     Discord APIのスタンプ情報を保持するクラス。
    /// </summary>
    internal sealed class DiscordSticker
    {
        [JsonInclude, JsonPropertyName("id")]
        internal string Id { get; set; } = string.Empty;

        [JsonInclude, JsonPropertyName("name")]
        internal string Name { get; set; } = string.Empty;
    }

    /// <summary>
    ///     Discord APIのスレッド一覧レスポンスを保持するクラス。
    /// </summary>
    internal sealed class DiscordThreadListResponse
    {
        [JsonInclude, JsonPropertyName("threads")]
        internal List<DiscordChannel> Threads { get; set; } = new();

        [JsonInclude, JsonPropertyName("has_more")]
        internal bool HasMore { get; set; }
    }

    /// <summary>
    ///     Discord APIのレート制限レスポンスを保持するクラス。
    /// </summary>
    internal sealed class DiscordRateLimitResponse
    {
        [JsonInclude, JsonPropertyName("retry_after")]
        internal double RetryAfterSeconds { get; set; }
    }
}
