using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SinfoniaStudio.NotionMarkdownExporter;

namespace SinfoniaStudio.NotionMarkdownWriter
{
    /// <summary>
    ///     テキストとページメンションが混ざった短い1行を、Notion APIのrich_text配列へ変換するクラス。
    ///     appendコマンドの入力にだけ使う簡易パーサーであり、Markdown Content APIが持つ書式（太字・表など）は扱わない。
    /// </summary>
    internal static class RichTextParser
    {
        // <mention-page url="...">表示テキスト</mention-page> と <mention-page url="..."/> の両方を受け付ける。
        private static readonly Regex _mentionPattern = new(
            @"<mention-page\s+url=""(?<url>[^""]+)""\s*(?:/>|>(?<text>[^<]*)</mention-page>)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        ///     入力文字列をrich_text配列へ変換する。
        /// </summary>
        /// <param name="input">テキストと`&lt;mention-page&gt;`タグが混ざった1行。</param>
        /// <returns>Notion APIへ送るrich_textの配列。</returns>
        internal static List<Dictionary<string, object>> Parse(string input)
        {
            List<Dictionary<string, object>> runs = new();
            int cursor = 0;
            foreach (Match match in _mentionPattern.Matches(input))
            {
                if (match.Index > cursor)
                {
                    AppendTextRun(runs, input[cursor..match.Index]);
                }

                if (!NotionIdentifier.TryExtract(match.Groups["url"].Value, out string pageId))
                {
                    throw new WriterException($"mention-pageのurlをNotionのページIDとして解釈できません: {match.Groups["url"].Value}");
                }

                runs.Add(new Dictionary<string, object>
                {
                    ["type"] = "mention",
                    ["mention"] = new Dictionary<string, object>
                    {
                        ["type"] = "page",
                        ["page"] = new Dictionary<string, string> { ["id"] = pageId }
                    }
                });

                cursor = match.Index + match.Length;
            }

            if (cursor < input.Length)
            {
                AppendTextRun(runs, input[cursor..]);
            }

            if (runs.Count == 0)
            {
                throw new WriterException("追加する内容が空です。");
            }

            return runs;
        }

        /// <summary>
        ///     空でなければプレーンテキストの1件をrich_text配列へ足す。
        /// </summary>
        /// <param name="runs">追加先。</param>
        /// <param name="text">テキスト。</param>
        private static void AppendTextRun(List<Dictionary<string, object>> runs, string text)
        {
            if (text.Length == 0) { return; }

            runs.Add(new Dictionary<string, object>
            {
                ["type"] = "text",
                ["text"] = new Dictionary<string, string> { ["content"] = text }
            });
        }
    }
}
