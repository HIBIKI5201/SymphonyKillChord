using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SinfoniaStudio.DiscordLogExporter
{
    /// <summary>
    ///     Discordのチャンネルまたはフォーラム単位でログファイルを生成するクラス。
    /// </summary>
    internal sealed class DiscordLogExporter
    {
        private const int GUILD_TEXT_CHANNEL = 0;
        private const int GUILD_ANNOUNCEMENT_CHANNEL = 5;
        private const int ANNOUNCEMENT_THREAD_CHANNEL = 10;
        private const int PUBLIC_THREAD_CHANNEL = 11;
        private const int PRIVATE_THREAD_CHANNEL = 12;
        private const int GUILD_FORUM_CHANNEL = 15;
        private const int GUILD_MEDIA_CHANNEL = 16;

        private readonly DiscordApiClient _apiClient;
        private readonly ExporterOptions _options;
        private int _warningCount;

        /// <summary>
        ///     Discord APIクライアントと実行設定からエクスポーターを生成する。
        /// </summary>
        /// <param name="apiClient">Discord APIクライアント。</param>
        /// <param name="options">実行設定。</param>
        internal DiscordLogExporter(DiscordApiClient apiClient, ExporterOptions options)
        {
            _apiClient = apiClient;
            _options = options;
        }

        /// <summary>
        ///     設定された全チャンネルをDocs/DiscordLogへ出力する。
        /// </summary>
        /// <returns>エクスポート結果の集計。</returns>
        internal async Task<ExportSummary> ExportAsync()
        {
            Directory.CreateDirectory(_options.OutputDirectory);
            int exportedChannelCount = 0;
            int threadCount = 0;
            int messageCount = 0;

            foreach (ulong channelId in _options.ChannelIds)
            {
                try
                {
                    DiscordChannel channel = await _apiClient.GetChannelAsync(channelId);
                    if (IsForumChannel(channel.Type))
                    {
                        (int exportedThreads, int exportedMessages) = await ExportForumChannelAsync(channel);
                        threadCount += exportedThreads;
                        messageCount += exportedMessages;
                        exportedChannelCount++;
                        continue;
                    }

                    if (IsMessageChannel(channel.Type))
                    {
                        messageCount += await ExportMessageChannelAsync(channel);
                        exportedChannelCount++;
                        continue;
                    }

                    Warn($"サポートされていないチャンネル種別です: {GetChannelName(channel)} " +
                         $"(ID: {channel.Id}, Type: {channel.Type})");
                }
                catch (Exception ex)
                {
                    Warn($"チャンネル {channelId} の出力に失敗しました: {ex.Message}");
                }
            }

            return new ExportSummary(
                exportedChannelCount,
                threadCount,
                messageCount,
                _warningCount,
                _options.OutputDirectory);
        }

        /// <summary>
        ///     通常チャンネルまたは単一スレッドの全メッセージを1ファイルへ出力する。
        /// </summary>
        /// <param name="channel">出力対象チャンネル。</param>
        /// <returns>出力したメッセージ数。</returns>
        private async Task<int> ExportMessageChannelAsync(DiscordChannel channel)
        {
            string channelName = GetChannelName(channel);
            Console.WriteLine($"[処理中] チャンネル: {channelName} (ID: {channel.Id})");
            ulong channelId = ParseChannelId(channel.Id);
            IReadOnlyList<DiscordMessage> messages = await _apiClient.GetMessagesAsync(channelId);
            EnsureMessageContentIsAvailable(messages, channelName);
            string outputPath = CreateOutputPath(channel);

            await WriteAtomicallyAsync(outputPath, async writer =>
            {
                await writer.WriteLineAsync($"=== チャンネル名: {channelName} (ID: {channel.Id}) ===");
                await writer.WriteLineAsync($"=== 出力日時: {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz} ===");
                await writer.WriteLineAsync($"=== メッセージ数: {messages.Count} ===");
                await writer.WriteLineAsync();
                foreach (DiscordMessage message in messages)
                {
                    await WriteMessageAsync(writer, message);
                }
            });

            Console.WriteLine($"[完了] 出力先: {outputPath}");
            return messages.Count;
        }

        /// <summary>
        ///     フォーラム内の投稿スレッドごとに独立したログファイルを出力する。
        /// </summary>
        /// <param name="forumChannel">出力対象フォーラム。</param>
        /// <returns>出力したスレッド数とメッセージ数。</returns>
        private async Task<(int ThreadCount, int MessageCount)> ExportForumChannelAsync(DiscordChannel forumChannel)
        {
            string forumName = GetChannelName(forumChannel);
            Console.WriteLine($"[処理中] フォーラム: {forumName} (ID: {forumChannel.Id})");
            IReadOnlyList<DiscordChannel> threads = await _apiClient.GetForumThreadsAsync(forumChannel);
            List<ForumThreadLog> threadLogs = new();
            int messageCount = 0;

            foreach (DiscordChannel thread in threads)
            {
                string threadName = GetChannelName(thread);
                try
                {
                    ulong threadId = ParseChannelId(thread.Id);
                    IReadOnlyList<DiscordMessage> messages = await _apiClient.GetMessagesAsync(threadId);
                    messageCount += messages.Count;
                    threadLogs.Add(new ForumThreadLog(thread, messages));
                }
                catch (Exception ex)
                {
                    Warn($"フォーラム '{forumName}' の投稿 '{threadName}' を取得できませんでした: {ex.Message}");
                }
            }

            if (threads.Count > 0 && threadLogs.Count == 0)
            {
                throw new InvalidOperationException($"フォーラム '{forumName}' の投稿を1件も取得できませんでした。");
            }

            EnsureForumMessageContentIsAvailable(threadLogs, forumName, messageCount);

            foreach (ForumThreadLog threadLog in threadLogs)
            {
                string threadName = GetChannelName(threadLog.Thread);
                string outputPath = CreateForumThreadOutputPath(forumChannel, threadLog.Thread);
                await WriteAtomicallyAsync(outputPath, async writer =>
                {
                    await writer.WriteLineAsync($"=== フォーラム名: {forumName} (ID: {forumChannel.Id}) ===");
                    await writer.WriteLineAsync($"=== ページ名: {threadName} (ID: {threadLog.Thread.Id}) ===");
                    await writer.WriteLineAsync($"=== 出力日時: {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz} ===");
                    await writer.WriteLineAsync($"=== メッセージ数: {threadLog.Messages.Count} ===");
                    await writer.WriteLineAsync();
                    foreach (DiscordMessage message in threadLog.Messages)
                    {
                        await WriteMessageAsync(writer, message);
                    }
                });

                Console.WriteLine($"[完了(フォーラムページ)] 出力先: {outputPath}");
            }

            DeleteLegacyForumFile(forumChannel);
            return (threadLogs.Count, messageCount);
        }

        /// <summary>
        ///     Discordメッセージをテキストログ形式で出力する。
        /// </summary>
        /// <param name="writer">出力先ライター。</param>
        /// <param name="message">Discordメッセージ。</param>
        private static async Task WriteMessageAsync(StreamWriter writer, DiscordMessage message)
        {
            string edited = message.EditedTimestamp.HasValue ? " (編集済み)" : string.Empty;
            await writer.WriteLineAsync(
                $"[{message.Timestamp.ToLocalTime():yyyy/MM/dd HH:mm:ss}] {GetAuthorName(message.Author)}{edited}:");

            if (!string.IsNullOrEmpty(message.Content))
            {
                await writer.WriteLineAsync(message.Content);
            }

            foreach (DiscordAttachment attachment in message.Attachments)
            {
                await writer.WriteLineAsync($"[添付ファイル] {attachment.FileName}: {attachment.Url}");
            }

            foreach (DiscordEmbed embed in message.Embeds)
            {
                string[] values = new[] { embed.Title, embed.Description, embed.Url }
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => value!)
                    .ToArray();
                if (values.Length > 0)
                {
                    await writer.WriteLineAsync($"[埋め込み] {string.Join(" | ", values)}");
                }
            }

            foreach (DiscordSticker sticker in message.Stickers)
            {
                await writer.WriteLineAsync($"[スタンプ] {sticker.Name} (ID: {sticker.Id})");
            }

            await writer.WriteLineAsync();
        }

        /// <summary>
        ///     取得したメッセージにDiscordから本文データが提供されているか検証する。
        /// </summary>
        /// <param name="messages">取得したメッセージ一覧。</param>
        /// <param name="channelName">診断メッセージに表示するチャンネル名。</param>
        private static void EnsureMessageContentIsAvailable(
            IReadOnlyList<DiscordMessage> messages,
            string channelName)
        {
            if (messages.Count == 0 || messages.Any(HasMessageContentData)) { return; }

            throw CreateMessageContentIntentException(channelName, messages.Count);
        }

        /// <summary>
        ///     フォーラム全体でDiscordから本文データが提供されているか検証する。
        /// </summary>
        /// <param name="threadLogs">フォーラムページごとの取得結果。</param>
        /// <param name="forumName">診断メッセージに表示するフォーラム名。</param>
        /// <param name="messageCount">フォーラム内の総メッセージ数。</param>
        private static void EnsureForumMessageContentIsAvailable(
            IReadOnlyList<ForumThreadLog> threadLogs,
            string forumName,
            int messageCount)
        {
            if (messageCount == 0 || threadLogs.Any(threadLog => threadLog.Messages.Any(HasMessageContentData)))
            {
                return;
            }

            throw CreateMessageContentIntentException(forumName, messageCount);
        }

        /// <summary>
        ///     Message Content Intentの対象となる本文データがメッセージにあるか判定する。
        /// </summary>
        /// <param name="message">判定対象メッセージ。</param>
        /// <returns>本文、添付、埋め込みのいずれかがある場合はtrue。</returns>
        private static bool HasMessageContentData(DiscordMessage message)
        {
            return !string.IsNullOrEmpty(message.Content) ||
                   message.Attachments.Count > 0 ||
                   message.Embeds.Count > 0;
        }

        /// <summary>
        ///     Message Content Intentが無効な可能性を示す診断例外を生成する。
        /// </summary>
        /// <param name="channelName">チャンネルまたはフォーラム名。</param>
        /// <param name="messageCount">本文が空だったメッセージ数。</param>
        /// <returns>設定手順を含む例外。</returns>
        private static InvalidOperationException CreateMessageContentIntentException(
            string channelName,
            int messageCount)
        {
            return new InvalidOperationException(
                $"'{channelName}' のメッセージ{messageCount}件すべてで本文データが空でした。" +
                "Discord Developer Portalの Bot > Privileged Gateway Intents で " +
                "Message Content Intentを有効にしてから再実行してください。");
        }

        /// <summary>
        ///     一時ファイルへ書き込んだ後、出力ファイルを置き換える。
        /// </summary>
        /// <param name="outputPath">最終出力パス。</param>
        /// <param name="writeAction">ファイル内容を書き込む処理。</param>
        private static async Task WriteAtomicallyAsync(string outputPath, Func<StreamWriter, Task> writeAction)
        {
            string directory = Path.GetDirectoryName(outputPath) ?? Directory.GetCurrentDirectory();
            string temporaryPath = Path.Combine(directory, $".{Guid.NewGuid():N}.tmp");
            try
            {
                await using (FileStream stream = new(
                    temporaryPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    81920,
                    true))
                await using (StreamWriter writer = new(stream, new UTF8Encoding(false)))
                {
                    await writeAction(writer);
                }

                File.Move(temporaryPath, outputPath, true);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }

        /// <summary>
        ///     チャンネル情報から安全な出力パスを生成する。
        /// </summary>
        /// <param name="channel">対象チャンネル。</param>
        /// <returns>出力ファイルの絶対パス。</returns>
        private string CreateOutputPath(DiscordChannel channel)
        {
            string safeName = CreateSafeFileName(GetChannelName(channel));
            return Path.Combine(_options.OutputDirectory, $"{safeName}_{channel.Id}.txt");
        }

        /// <summary>
        ///     フォーラム名、ページ名、ページIDから安全な出力パスを生成する。
        /// </summary>
        /// <param name="forumChannel">親フォーラム。</param>
        /// <param name="thread">フォーラムページ。</param>
        /// <returns>フォーラムページ用ファイルの絶対パス。</returns>
        private string CreateForumThreadOutputPath(DiscordChannel forumChannel, DiscordChannel thread)
        {
            string forumName = CreateSafeFileName(GetChannelName(forumChannel));
            string threadName = CreateSafeFileName(GetChannelName(thread));
            return Path.Combine(_options.OutputDirectory, $"{forumName}_{threadName}_{thread.Id}.txt");
        }

        /// <summary>
        ///     旧バージョンが生成したフォーラム単位の結合ログを、安全性を確認して削除する。
        /// </summary>
        /// <param name="forumChannel">親フォーラム。</param>
        private void DeleteLegacyForumFile(DiscordChannel forumChannel)
        {
            string legacyPath = CreateOutputPath(forumChannel);
            if (!File.Exists(legacyPath)) { return; }

            using StreamReader reader = new(legacyPath, Encoding.UTF8, true);
            string? firstLine = reader.ReadLine();
            if (firstLine == null || !firstLine.StartsWith("=== フォーラム名:", StringComparison.Ordinal))
            {
                Warn($"旧フォーラムログと判定できないため削除をスキップしました: {legacyPath}");
                return;
            }

            reader.Close();
            File.Delete(legacyPath);
            Console.WriteLine($"[整理] 旧形式のフォーラムログを削除しました: {legacyPath}");
        }

        /// <summary>
        ///     Windowsでファイル名に使用できない文字をアンダースコアへ置換する。
        /// </summary>
        /// <param name="value">元の文字列。</param>
        /// <returns>安全なファイル名。</returns>
        private static string CreateSafeFileName(string value)
        {
            StringBuilder result = new(value.Trim());
            foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
            {
                result.Replace(invalidCharacter, '_');
            }

            string safeName = result.ToString().TrimEnd('.', ' ');
            return string.IsNullOrWhiteSpace(safeName) ? "channel" : safeName;
        }

        /// <summary>
        ///     チャンネル名を取得し、未設定の場合は代替名を返す。
        /// </summary>
        /// <param name="channel">対象チャンネル。</param>
        /// <returns>表示用チャンネル名。</returns>
        private static string GetChannelName(DiscordChannel channel)
        {
            return string.IsNullOrWhiteSpace(channel.Name) ? $"channel-{channel.Id}" : channel.Name;
        }

        /// <summary>
        ///     Discordユーザーの表示名をログ用に整形する。
        /// </summary>
        /// <param name="author">投稿者情報。</param>
        /// <returns>投稿者の表示名。</returns>
        private static string GetAuthorName(DiscordUser author)
        {
            string name = string.IsNullOrWhiteSpace(author.GlobalName) ? author.Username : author.GlobalName;
            if (!string.IsNullOrWhiteSpace(author.Discriminator) && author.Discriminator != "0")
            {
                name += $"#{author.Discriminator}";
            }

            return author.IsBot ? $"{name} [Bot]" : name;
        }

        /// <summary>
        ///     Discord APIのチャンネルIDを数値へ変換する。
        /// </summary>
        /// <param name="channelId">チャンネルID文字列。</param>
        /// <returns>数値のチャンネルID。</returns>
        private static ulong ParseChannelId(string channelId)
        {
            if (ulong.TryParse(channelId, out ulong result)) { return result; }
            throw new InvalidOperationException($"Discord APIが無効なチャンネルIDを返しました: {channelId}");
        }

        /// <summary>
        ///     メッセージ履歴を取得できるチャンネル種別か判定する。
        /// </summary>
        /// <param name="channelType">Discordのチャンネル種別値。</param>
        /// <returns>対応する場合はtrue。</returns>
        private static bool IsMessageChannel(int channelType)
        {
            return channelType is GUILD_TEXT_CHANNEL or
                GUILD_ANNOUNCEMENT_CHANNEL or
                ANNOUNCEMENT_THREAD_CHANNEL or
                PUBLIC_THREAD_CHANNEL or
                PRIVATE_THREAD_CHANNEL;
        }

        /// <summary>
        ///     フォーラムと同じ形式で出力するチャンネル種別か判定する。
        /// </summary>
        /// <param name="channelType">Discordのチャンネル種別値。</param>
        /// <returns>フォーラム形式の場合はtrue。</returns>
        private static bool IsForumChannel(int channelType)
        {
            return channelType is GUILD_FORUM_CHANNEL or GUILD_MEDIA_CHANNEL;
        }

        /// <summary>
        ///     警告件数を加算し、標準エラー出力へ表示する。
        /// </summary>
        /// <param name="message">警告内容。</param>
        private void Warn(string message)
        {
            _warningCount++;
            Console.Error.WriteLine($"警告: {message}");
        }

        /// <summary>
        ///     フォーラムページと取得したメッセージを対応付けるクラス。
        /// </summary>
        private sealed class ForumThreadLog
        {
            /// <summary>
            ///     フォーラムページの取得結果を生成する。
            /// </summary>
            /// <param name="thread">フォーラムページ。</param>
            /// <param name="messages">ページ内の全メッセージ。</param>
            internal ForumThreadLog(DiscordChannel thread, IReadOnlyList<DiscordMessage> messages)
            {
                Thread = thread;
                Messages = messages;
            }

            internal DiscordChannel Thread { get; }
            internal IReadOnlyList<DiscordMessage> Messages { get; }
        }
    }

    /// <summary>
    ///     Discordログエクスポートの集計結果を保持するクラス。
    /// </summary>
    internal sealed class ExportSummary
    {
        /// <summary>
        ///     エクスポート結果を生成する。
        /// </summary>
        /// <param name="exportedChannelCount">出力したチャンネルまたはフォーラム数。</param>
        /// <param name="threadCount">出力したフォーラム投稿数。</param>
        /// <param name="messageCount">出力したメッセージ数。</param>
        /// <param name="warningCount">警告件数。</param>
        /// <param name="outputDirectory">出力先。</param>
        internal ExportSummary(
            int exportedChannelCount,
            int threadCount,
            int messageCount,
            int warningCount,
            string outputDirectory)
        {
            ExportedChannelCount = exportedChannelCount;
            ThreadCount = threadCount;
            MessageCount = messageCount;
            WarningCount = warningCount;
            OutputDirectory = outputDirectory;
        }

        internal int ExportedChannelCount { get; }
        internal int ThreadCount { get; }
        internal int MessageCount { get; }
        internal int WarningCount { get; }
        internal string OutputDirectory { get; }
    }
}
