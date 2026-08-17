using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SinfoniaStudio.NotionMarkdownWriter
{
    /// <summary>
    ///     編集した作業ファイルを部分置換としてNotionへ反映するコマンド。
    /// </summary>
    internal static class PushCommand
    {
        private static readonly string[] _valueOptions = Array.Empty<string>();
        private static readonly string[] _flagOptions = { "confirm", "quiet", "whole" };

        /// <summary> 差分表示で1件あたりに出力する最大行数。 </summary>
        private const int MAX_PREVIEW_LINE_COUNT = 40;

        /// <summary>
        ///     pushコマンドを実行する。
        /// </summary>
        /// <param name="args">サブコマンド名を除いた引数。</param>
        /// <returns>正常終了時は0。</returns>
        internal static async Task<int> RunAsync(string[] args)
        {
            CommandArguments arguments = CommandArguments.Parse(args, _valueOptions, _flagOptions);
            string workFilePath = Path.GetFullPath(arguments.GetRequiredOperand("作業ファイル"));
            if (!File.Exists(workFilePath)) { throw new WriterException($"作業ファイルが見つかりません: {workFilePath}"); }

            bool isConfirmed = arguments.HasFlag("confirm");
            bool isQuiet = arguments.HasFlag("quiet");
            bool replacesWholeBody = arguments.HasFlag("whole");
            WriterEnvironment environment = WriterEnvironment.Load();
            PullSnapshot snapshot = PullSnapshot.Load(workFilePath);

            string baseline = MarkdownDiffBuilder.Normalize(snapshot.Baseline);
            string edited = MarkdownDiffBuilder.Normalize(await File.ReadAllTextAsync(workFilePath));
            if (string.Equals(baseline, edited, StringComparison.Ordinal))
            {
                Console.WriteLine("変更がありません。");
                return 0;
            }

            IReadOnlyList<ContentUpdate> updates = MarkdownDiffBuilder.Build(baseline, edited);
            if (updates.Count == 0)
            {
                Console.WriteLine("変更がありません。");
                return 0;
            }

            using NotionWriteClient client = new(environment.NotionToken);
            WriteScopeGuard guard = new(environment.AllowedRootPageIds, client);
            NotionPageInfo page = await client.GetPageAsync(snapshot.PageId);
            string allowedRootId = await guard.AuthorizeEditAsync(page);

            // pull以降にNotion側が更新されていると、置換前文字列が現在の本文と食い違う。
            if (!string.Equals(page.LastEditedTime, snapshot.LastEditedTime, StringComparison.Ordinal))
            {
                throw new WriterException(
                    $"pull以降にNotion側が更新されています（pull時: {snapshot.LastEditedTime} / 現在: {page.LastEditedTime}）。" +
                    "pullし直してから編集内容を作り直してください。");
            }

            WritePlan(page, allowedRootId, updates);

            // 全面刷新では置換が多数に分かれ、途中で一致しなくなると中途半端に適用されうる。
            // 本文全体を1件の置換として送ることで、適用の成否をページ単位に揃える。
            IReadOnlyList<ContentUpdate> sentUpdates = replacesWholeBody
                ? new[] { new ContentUpdate(baseline, edited) }
                : updates;
            if (replacesWholeBody)
            {
                Console.WriteLine();
                Console.WriteLine("--whole が指定されているため、上記をまとめて本文全体の置換1件として送信します。");
            }

            if (!isConfirmed)
            {
                Console.WriteLine();
                Console.WriteLine("送信していません。内容を確認し、--confirm を付けて再実行してください。");
                return 0;
            }

            await client.UpdateMarkdownAsync(page.Id, sentUpdates);
            Console.WriteLine();
            Console.WriteLine($"{sentUpdates.Count}件の変更を反映しました: {page.Url}");

            await VerifyAsync(client, workFilePath, snapshot, edited, isQuiet);
            return 0;
        }

        /// <summary>
        ///     送信予定の内容を表示する。
        /// </summary>
        /// <param name="page">対象ページ。</param>
        /// <param name="allowedRootId">一致した許可ルートページID。</param>
        /// <param name="updates">置換一覧。</param>
        private static void WritePlan(NotionPageInfo page, string allowedRootId, IReadOnlyList<ContentUpdate> updates)
        {
            Console.WriteLine($"対象ページ: {page.Title}");
            Console.WriteLine($"URL: {page.Url}");
            Console.WriteLine($"許可ルート: {allowedRootId}");
            Console.WriteLine($"変更点: {updates.Count}件（update_contentによる部分置換）");

            for (int index = 0; index < updates.Count; index++)
            {
                Console.WriteLine();
                Console.WriteLine($"--- [{index + 1}/{updates.Count}] ---");
                WriteDiffLines(updates[index].OldString, '-');
                WriteDiffLines(updates[index].NewString, '+');
            }
        }

        /// <summary>
        ///     置換前後の内容を差分形式で表示する。
        /// </summary>
        /// <param name="text">表示するテキスト。</param>
        /// <param name="marker">行頭に付ける記号。</param>
        private static void WriteDiffLines(string text, char marker)
        {
            string[] lines = text.Split('\n');
            int printedCount = Math.Min(lines.Length, MAX_PREVIEW_LINE_COUNT);
            for (int index = 0; index < printedCount; index++)
            {
                Console.WriteLine($"{marker}{lines[index]}");
            }

            if (lines.Length > printedCount)
            {
                Console.WriteLine($"{marker}... 他{lines.Length - printedCount}行");
            }
        }

        /// <summary>
        ///     反映後の本文を取得し直し、作業ファイルとpull情報を最新化する。
        /// </summary>
        /// <param name="client">APIクライアント。</param>
        /// <param name="workFilePath">作業ファイルのパス。</param>
        /// <param name="snapshot">更新するpull情報。</param>
        /// <param name="expected">送信した編集後の内容。</param>
        /// <param name="isQuiet">確認結果の詳細表示を抑えるかどうか。</param>
        private static async Task VerifyAsync(
            NotionWriteClient client,
            string workFilePath,
            PullSnapshot snapshot,
            string expected,
            bool isQuiet)
        {
            NotionPageInfo updatedPage = await client.GetPageAsync(snapshot.PageId);
            string current = MarkdownDiffBuilder.Normalize(await client.GetMarkdownAsync(snapshot.PageId));

            await File.WriteAllTextAsync(workFilePath, current, new UTF8Encoding(false));
            snapshot.LastEditedTime = updatedPage.LastEditedTime;
            snapshot.PulledAtUtc = DateTimeOffset.UtcNow;
            snapshot.Baseline = current;
            snapshot.Save(workFilePath);

            if (string.Equals(current, expected, StringComparison.Ordinal))
            {
                if (!isQuiet) { Console.WriteLine("反映後の本文が編集内容と一致することを確認しました。"); }
                return;
            }

            // Notionはブロックへ変換した結果を返すため、書式の正規化で差が出ることがある。
            Console.WriteLine(
                "反映後の本文が編集内容と完全には一致しません（Notion側の書式正規化の可能性があります）。" +
                "作業ファイルは最新の本文で更新済みです。差分を確認してください。");
        }
    }
}
