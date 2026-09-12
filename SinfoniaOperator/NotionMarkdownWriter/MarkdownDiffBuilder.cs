using System;
using System.Collections.Generic;
using System.Linq;

namespace SinfoniaStudio.NotionMarkdownWriter
{
    /// <summary>
    ///     Markdown Content APIのcontent_updates 1件分を保持するクラス。
    /// </summary>
    internal sealed class ContentUpdate
    {
        /// <summary>
        ///     置換内容を生成する。
        /// </summary>
        /// <param name="oldString">置換前の文字列。ページ内で一意であること。</param>
        /// <param name="newString">置換後の文字列。</param>
        internal ContentUpdate(string oldString, string newString)
        {
            OldString = oldString;
            NewString = newString;
        }

        internal string OldString { get; }
        internal string NewString { get; }
    }

    /// <summary>
    ///     編集前後のMarkdownから、Notionへ送る部分置換の一覧を組み立てるクラス。
    ///     old_strはページ内で一意でなければならないため、一意になるまで前後の行を文脈として足す。
    /// </summary>
    internal static class MarkdownDiffBuilder
    {
        /// <summary> 差分計算に用いるDPテーブルのメモリを抑えるための行数上限。 </summary>
        private const int MAX_LINE_COUNT = 2000;

        /// <summary>
        ///     改行コードをLFへ揃える。Notionが返す原文とローカル編集結果を同じ土俵で比較するため。
        /// </summary>
        /// <param name="text">対象テキスト。</param>
        /// <returns>LFへ正規化したテキスト。</returns>
        internal static string Normalize(string text)
        {
            return text.Replace("\r\n", "\n", StringComparison.Ordinal)
                       .Replace("\r", "\n", StringComparison.Ordinal);
        }

        /// <summary>
        ///     編集前後のMarkdownから置換一覧を組み立てる。
        /// </summary>
        /// <param name="baseline">pull時点のMarkdown原文。</param>
        /// <param name="edited">編集後のMarkdown。</param>
        /// <returns>適用順に並んだ置換一覧。差分が無い場合は空。</returns>
        internal static IReadOnlyList<ContentUpdate> Build(string baseline, string edited)
        {
            string[] baseLines = Normalize(baseline).Split('\n');
            string[] editLines = Normalize(edited).Split('\n');
            if (baseLines.Length > MAX_LINE_COUNT || editLines.Length > MAX_LINE_COUNT)
            {
                throw new WriterException(
                    $"ページの行数が{MAX_LINE_COUNT}行を超えています。編集範囲を分けるか、Notion上で直接編集してください。");
            }

            string normalizedBaseline = string.Join("\n", baseLines);
            List<Hunk> hunks = CreateHunks(baseLines, editLines);
            List<Hunk> resolvedHunks = new();
            foreach (Hunk hunk in hunks)
            {
                Hunk resolved = Expand(hunk, baseLines, editLines, normalizedBaseline);

                // 文脈を足した結果、直前の置換範囲と重なった場合は統合してもう一度一意化する。
                while (resolvedHunks.Count > 0 && Overlaps(resolvedHunks[^1], resolved))
                {
                    Hunk merged = Merge(resolvedHunks[^1], resolved);
                    resolvedHunks.RemoveAt(resolvedHunks.Count - 1);
                    resolved = Expand(merged, baseLines, editLines, normalizedBaseline);
                }

                resolvedHunks.Add(resolved);
            }

            List<ContentUpdate> updates = resolvedHunks
                .Select(hunk => new ContentUpdate(
                    Join(baseLines, hunk.BaseStart, hunk.BaseEnd),
                    Join(editLines, hunk.EditStart, hunk.EditEnd)))
                .ToList();
            ValidateSequentialApplication(normalizedBaseline, updates);
            return updates;
        }

        /// <summary>
        ///     置換を順に適用した本文に対して、各置換対象が一意であることを検証する。
        ///     Notionは content_updates を順番に適用するため、pull時点の本文だけで一意性を見ると、
        ///     先行する置換が生んだ文面や消した文面によって後続が誤った位置へ当たりうる。
        /// </summary>
        /// <param name="baseline">pull時点の本文。</param>
        /// <param name="updates">適用する置換。</param>
        private static void ValidateSequentialApplication(string baseline, IReadOnlyList<ContentUpdate> updates)
        {
            string current = baseline;
            for (int index = 0; index < updates.Count; index++)
            {
                ContentUpdate update = updates[index];
                int count = CountOccurrences(current, update.OldString);
                if (count != 1)
                {
                    throw new WriterException(
                        $"{index + 1}件目の置換対象が、先行する置換を適用した後の本文中で{count}箇所見つかりました。" +
                        "このまま送るとNotion側で意図しない位置へ適用されます。" +
                        "--whole を付けて本文全体の置換として送信してください。");
                }

                int position = current.IndexOf(update.OldString, StringComparison.Ordinal);
                current = current[..position] + update.NewString + current[(position + update.OldString.Length)..];
            }
        }

        /// <summary>
        ///     最長共通部分列を基に、変更のある区間を抽出する。
        /// </summary>
        /// <param name="baseLines">編集前の行。</param>
        /// <param name="editLines">編集後の行。</param>
        /// <returns>変更区間の一覧。</returns>
        private static List<Hunk> CreateHunks(string[] baseLines, string[] editLines)
        {
            int baseCount = baseLines.Length;
            int editCount = editLines.Length;
            int[,] commonLength = new int[baseCount + 1, editCount + 1];
            for (int baseIndex = baseCount - 1; baseIndex >= 0; baseIndex--)
            {
                for (int editIndex = editCount - 1; editIndex >= 0; editIndex--)
                {
                    commonLength[baseIndex, editIndex] = IsSameLine(baseLines[baseIndex], editLines[editIndex])
                        ? commonLength[baseIndex + 1, editIndex + 1] + 1
                        : Math.Max(commonLength[baseIndex + 1, editIndex], commonLength[baseIndex, editIndex + 1]);
                }
            }

            List<Hunk> hunks = new();
            int currentBase = 0;
            int currentEdit = 0;
            while (currentBase < baseCount || currentEdit < editCount)
            {
                if (currentBase < baseCount && currentEdit < editCount &&
                    IsSameLine(baseLines[currentBase], editLines[currentEdit]))
                {
                    currentBase++;
                    currentEdit++;
                    continue;
                }

                int hunkBaseStart = currentBase;
                int hunkEditStart = currentEdit;
                while (currentBase < baseCount || currentEdit < editCount)
                {
                    if (currentBase < baseCount && currentEdit < editCount &&
                        IsSameLine(baseLines[currentBase], editLines[currentEdit]))
                    {
                        break;
                    }

                    bool advancesBase = currentBase < baseCount &&
                                        (currentEdit >= editCount ||
                                         commonLength[currentBase + 1, currentEdit] >=
                                         commonLength[currentBase, currentEdit + 1]);
                    if (advancesBase) { currentBase++; }
                    else { currentEdit++; }
                }

                hunks.Add(new Hunk(hunkBaseStart, currentBase, hunkEditStart, currentEdit));
            }

            return hunks;
        }

        /// <summary>
        ///     置換前文字列がページ内で一意になるまで、変更区間へ前後の行を足す。
        /// </summary>
        /// <param name="hunk">変更区間。</param>
        /// <param name="baseLines">編集前の行。</param>
        /// <param name="editLines">編集後の行。</param>
        /// <param name="normalizedBaseline">一意性を数えるための編集前全文。</param>
        /// <returns>文脈を含めた変更区間。</returns>
        private static Hunk Expand(Hunk hunk, string[] baseLines, string[] editLines, string normalizedBaseline)
        {
            for (int context = 0; ; context++)
            {
                int baseStart = Math.Max(0, hunk.BaseStart - context);
                int baseEnd = Math.Min(baseLines.Length, hunk.BaseEnd + context);
                int editStart = Math.Max(0, hunk.EditStart - context);
                int editEnd = Math.Min(editLines.Length, hunk.EditEnd + context);
                string oldString = Join(baseLines, baseStart, baseEnd);
                if (oldString.Length > 0 && CountOccurrences(normalizedBaseline, oldString) == 1)
                {
                    return new Hunk(baseStart, baseEnd, editStart, editEnd);
                }

                if (baseStart != 0 || baseEnd != baseLines.Length) { continue; }

                if (oldString.Length == 0)
                {
                    throw new WriterException(
                        "編集前のページが空のため、部分更新の起点になる文字列がありません。" +
                        "空ページへの追記はこのツールでは行えません。");
                }

                throw new WriterException(
                    "ページ内で一意に特定できる置換範囲を作れませんでした。" +
                    "同じ文面が繰り返されている可能性があります。編集箇所を分けて試してください。");
            }
        }

        /// <summary>
        ///     ふたつの変更区間が重なっているかを判定する。
        /// </summary>
        /// <param name="first">先に確定した区間。</param>
        /// <param name="second">後続の区間。</param>
        /// <returns>重なっている場合はtrue。</returns>
        private static bool Overlaps(Hunk first, Hunk second)
        {
            return first.BaseEnd > second.BaseStart || first.EditEnd > second.EditStart;
        }

        /// <summary>
        ///     ふたつの変更区間を統合する。
        /// </summary>
        /// <param name="first">先に確定した区間。</param>
        /// <param name="second">後続の区間。</param>
        /// <returns>統合した区間。</returns>
        private static Hunk Merge(Hunk first, Hunk second)
        {
            return new Hunk(
                Math.Min(first.BaseStart, second.BaseStart),
                Math.Max(first.BaseEnd, second.BaseEnd),
                Math.Min(first.EditStart, second.EditStart),
                Math.Max(first.EditEnd, second.EditEnd));
        }

        /// <summary>
        ///     行範囲を改行で連結する。
        /// </summary>
        /// <param name="lines">行。</param>
        /// <param name="start">開始位置。</param>
        /// <param name="end">終了位置（含まない）。</param>
        /// <returns>連結した文字列。</returns>
        private static string Join(string[] lines, int start, int end)
        {
            return string.Join("\n", lines[start..end]);
        }

        /// <summary>
        ///     部分文字列の出現回数を数える。
        /// </summary>
        /// <param name="text">検索対象。</param>
        /// <param name="value">部分文字列。</param>
        /// <returns>出現回数。</returns>
        private static int CountOccurrences(string text, string value)
        {
            int count = 0;
            int index = 0;
            while (index <= text.Length - value.Length)
            {
                int found = text.IndexOf(value, index, StringComparison.Ordinal);
                if (found < 0) { break; }

                count++;
                index = found + 1;
            }

            return count;
        }

        /// <summary>
        ///     行が一致するかを判定する。
        /// </summary>
        /// <param name="left">左辺。</param>
        /// <param name="right">右辺。</param>
        /// <returns>一致する場合はtrue。</returns>
        private static bool IsSameLine(string left, string right)
        {
            return string.Equals(left, right, StringComparison.Ordinal);
        }

        /// <summary>
        ///     編集前後で対応する変更区間を保持する構造体。終了位置は含まない。
        /// </summary>
        private readonly struct Hunk
        {
            internal Hunk(int baseStart, int baseEnd, int editStart, int editEnd)
            {
                BaseStart = baseStart;
                BaseEnd = baseEnd;
                EditStart = editStart;
                EditEnd = editEnd;
            }

            internal int BaseStart { get; }
            internal int BaseEnd { get; }
            internal int EditStart { get; }
            internal int EditEnd { get; }
        }
    }
}
