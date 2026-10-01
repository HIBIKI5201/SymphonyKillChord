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

            // --whole は本文全体を1件で置き換えるため、部分差分を組み立てる必要がない。
            // 先に組み立てると、一意なold_strを作れない編集で全体置換へ到達する前に失敗する。
            IReadOnlyList<ContentUpdate> updates = replacesWholeBody
                ? new[] { new ContentUpdate(baseline, edited) }
                : MarkdownDiffBuilder.Build(baseline, edited);
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

            WritePlan(page, allowedRootId, updates, replacesWholeBody);

            if (!isConfirmed)
            {
                Console.WriteLine();
                Console.WriteLine("送信していません。内容を確認し、--confirm を付けて再実行してください。");
                return 0;
            }

            await client.UpdateMarkdownAsync(page.Id, updates);
            Console.WriteLine();
            Console.WriteLine($"{updates.Count}件の変更を反映しました: {page.Url}");

            await VerifyAsync(client, workFilePath, snapshot, edited, isQuiet);
            return 0;
        }

        /// <summary>
        ///     送信予定の内容を表示する。
        /// </summary>
        /// <param name="page">対象ページ。</param>
        /// <param name="allowedRootId">一致した許可ルートページID。</param>
        /// <param name="updates">置換一覧。</param>
        private static void WritePlan(
            NotionPageInfo page,
            string allowedRootId,
            IReadOnlyList<ContentUpdate> updates,
            bool replacesWholeBody)
        {
            Console.WriteLine($"対象ページ: {page.Title}");
            Console.WriteLine($"URL: {page.Url}");
            Console.WriteLine($"許可ルート: {allowedRootId}");
            Console.WriteLine(replacesWholeBody
                ? "変更点: 本文全体の置換1件（update_content）"
                : $"変更点: {updates.Count}件（update_contentによる部分置換）");

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
        ///     承認の判断材料になるため、省略せず全行を出す。
        /// </summary>
        /// <param name="text">表示するテキスト。</param>
        /// <param name="marker">行頭に付ける記号。</param>
        private static void WriteDiffLines(string text, char marker)
        {
            foreach (string line in text.Split('\n'))
            {
                Console.WriteLine($"{marker}{line}");
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
            bool isExactMatch = string.Equals(current, expected, StringComparison.Ordinal);
            bool isWhitespaceOnlyDifference = !isExactMatch && IsEquivalentIgnoringWhitespace(current, expected);
            string intendedFilePath = workFilePath + ".notion-push-intended.md";

            // Notionはブロックへ変換した結果を返すため、書式の正規化を超える差が出ることがある。
            // 作業ファイルは実際の反映結果で上書きするため、送ろうとした内容を先に別ファイルへ残す。
            // この保存に失敗した場合は、作業ファイルを上書きせずに止まる。
            if (!isExactMatch && !isWhitespaceOnlyDifference)
            {
                await File.WriteAllTextAsync(intendedFilePath, expected, new UTF8Encoding(false));
            }

            await File.WriteAllTextAsync(workFilePath, current, new UTF8Encoding(false));
            snapshot.LastEditedTime = updatedPage.LastEditedTime;
            snapshot.PulledAtUtc = DateTimeOffset.UtcNow;
            snapshot.Baseline = current;
            snapshot.Save(workFilePath);

            if (isExactMatch)
            {
                if (!isQuiet) { Console.WriteLine("反映後の本文が編集内容と一致することを確認しました。"); }
                return;
            }

            if (isWhitespaceOnlyDifference)
            {
                if (!isQuiet)
                {
                    Console.WriteLine(
                        "反映後の本文は、空白・改行の違いだけで編集内容と一致しています" +
                        "（Notion側の書式正規化）。作業ファイルは最新の本文で更新済みです。");
                }

                return;
            }

            Console.WriteLine(
                "反映後の本文が編集内容と完全には一致しません（Notion側の書式正規化を超える差の可能性があります）。" +
                $"作業ファイルは最新の本文で更新済みです。送ろうとした内容は {intendedFilePath} に残しています。差分を確認してください。");
        }

        /// <summary>
        ///     行末・空行の違いだけかを判定する。
        ///     Notionの書式正規化（末尾空白の除去など）を、内容の不一致と誤検知しないようにする。
        ///     ただし、2個以上の末尾スペースはCommonMarkの強制改行なので、その有無の違いは不一致として扱う。
        /// </summary>
        /// <param name="left">比較対象。</param>
        /// <param name="right">比較対象。</param>
        /// <returns>行ごとの空白差以外に違いが無ければtrue。</returns>
        private static bool IsEquivalentIgnoringWhitespace(string left, string right)
        {
            string[] leftLines = left.Split('\n');
            string[] rightLines = right.Split('\n');
            if (leftLines.Length != rightLines.Length) { return false; }

            for (int index = 0; index < leftLines.Length; index++)
            {
                if (leftLines[index].TrimEnd() != rightLines[index].TrimEnd()) { return false; }

                if (HasHardBreak(leftLines[index]) != HasHardBreak(rightLines[index])) { return false; }
            }

            return true;
        }

        /// <summary>
        ///     行末が強制改行（2個以上の半角スペース）かを判定する。
        /// </summary>
        /// <param name="line">判定する行。</param>
        /// <returns>強制改行ならtrue。</returns>
        private static bool HasHardBreak(string line)
        {
            return line.EndsWith("  ", StringComparison.Ordinal);
        }
    }
}
