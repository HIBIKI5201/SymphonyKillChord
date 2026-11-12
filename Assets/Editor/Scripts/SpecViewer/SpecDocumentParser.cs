using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace KillChord.Editor.SpecViewer
{
    /// <summary>
    ///     NotionMarkdownExporter が書き出したMarkdown（Notion形式）をブロックの木に分ける。
    ///     Notion形式は、子ブロックをタブの字下げで表し、トグル・コールアウト・段組み・表などをHTML風のタグで書く。
    ///     文中の書式（強調・リンクなど）はここでは解釈せず、<see cref="SpecInlineFormatter"/> に任せる。
    /// </summary>
    public sealed class SpecDocumentParser
    {
        /// <summary>
        ///     Markdownをブロックの木に分ける。
        /// </summary>
        /// <param name="markdown">ページのMarkdown。</param>
        /// <returns>最上位のブロック。</returns>
        public IReadOnlyList<SpecBlock> Parse(string markdown)
        {
            _lines.Clear();
            _index = 0;
            if (string.IsNullOrEmpty(markdown)) { return Array.Empty<SpecBlock>(); }

            foreach (string raw in markdown.Replace("\r\n", "\n").Split('\n'))
            {
                _lines.Add(new SourceLine(raw));
            }

            return ParseBlocks(0, null);
        }

        private const string DETAILS_CLOSE = "</details>";
        private const string CALLOUT_CLOSE = "</callout>";
        private const string COLUMNS_CLOSE = "</columns>";
        private const string COLUMN_CLOSE = "</column>";
        private const string TABLE_CLOSE = "</table>";
        private const string CELL_CLOSE = "</td>";

        private static readonly Regex HEADING = new(@"^(#{1,6})\s+(.*)$");
        private static readonly Regex BLOCK_ATTRIBUTE = new(@"\s*\{[a-z-]+=""[^""]*""\}\s*$");
        private static readonly Regex TASK_ITEM = new(@"^[-*+]\s+\[([ xX])\]\s*(.*)$");
        private static readonly Regex BULLET_ITEM = new(@"^[-*+]\s+(.*)$");
        private static readonly Regex NUMBERED_ITEM = new(@"^(\d+)[.)]\s+(.*)$");
        private static readonly Regex QUOTE = new(@"^>\s?(.*)$");
        private static readonly Regex DIVIDER = new(@"^(-{3,}|\*{3,}|_{3,})$");
        private static readonly Regex IMAGE = new(@"^!\[([^\]]*)\]\(([^)]*)\)\s*$");
        private static readonly Regex MEDIA = new(@"^<(video|audio|file)\s+src=""([^""]*)""[^>]*>(.*?)(</\1>)?$");
        private static readonly Regex NOTION_ONLY = new(@"^<unknown\s+url=""([^""]*)""(?:\s+alt=""([^""]*)"")?\s*/>");
        private static readonly Regex ATTRIBUTE = new(@"([a-z-]+)=""([^""]*)""");
        private static readonly Regex TAG = new(@"<[^>]+>");
        private static readonly Regex TABLE_SEPARATOR_CELL = new(@"^\s*:?-+:?\s*$");

        private readonly List<SourceLine> _lines = new();
        private int _index;

        /// <summary>
        ///     同じ字下げのブロックを、字下げが浅くなるか閉じタグに達するまで読む。
        /// </summary>
        /// <param name="indent">読むブロックの字下げ（タブの数）。</param>
        /// <param name="closingTag">読み終える閉じタグ。無ければ null。</param>
        /// <returns>読んだブロック。</returns>
        private List<SpecBlock> ParseBlocks(int indent, string closingTag)
        {
            List<SpecBlock> blocks = new();
            while (_index < _lines.Count)
            {
                SourceLine line = _lines[_index];
                if (line.IsBlank)
                {
                    _index++;
                    continue;
                }

                if (closingTag != null && line.Indent <= indent && line.Content.StartsWith(closingTag, StringComparison.Ordinal))
                {
                    _index++;
                    return blocks;
                }

                if (line.Indent < indent) { return blocks; }

                // 深く字下げされた行は、直前のブロックの子にする（Notionの子ブロック）。
                if (line.Indent > indent)
                {
                    List<SpecBlock> nested = ParseBlocks(line.Indent, null);
                    if (blocks.Count > 0) { blocks[blocks.Count - 1].AddChildren(nested); }
                    else { blocks.AddRange(nested); }
                    continue;
                }

                SpecBlock block = ParseBlock(line);
                if (block != null) { blocks.Add(block); }
            }

            return blocks;
        }

        /// <summary>
        ///     現在の行から1ブロックを読む。複数行にわたるブロック（タグで囲まれたもの・表・コード）は閉じるまで読む。
        /// </summary>
        /// <param name="line">現在の行。</param>
        /// <returns>読んだブロック。表示しない行なら null。</returns>
        private SpecBlock ParseBlock(SourceLine line)
        {
            string content = line.Content;
            _index++;

            if (content.StartsWith("<!--", StringComparison.Ordinal)) { return null; }
            if (content.StartsWith("```", StringComparison.Ordinal)) { return ParseCode(line); }
            if (content.StartsWith("|", StringComparison.Ordinal)) { return ParsePipeTable(line); }
            if (content.StartsWith("<", StringComparison.Ordinal))
            {
                SpecBlock tagBlock = ParseTagBlock(line);
                if (tagBlock != null || content.StartsWith("</", StringComparison.Ordinal)) { return tagBlock; }
            }

            Match match = HEADING.Match(content);
            if (match.Success)
            {
                string text = match.Groups[2].Value;
                bool isToggle = text.Contains("{toggle=\"true\"}");
                while (BLOCK_ATTRIBUTE.IsMatch(text)) { text = BLOCK_ATTRIBUTE.Replace(text, string.Empty); }
                SpecBlock heading = new SpecBlock(SpecBlockKind.Heading, text).SetLevel(match.Groups[1].Length);
                return isToggle ? heading.MarkToggle() : heading;
            }

            if ((match = TASK_ITEM.Match(content)).Success)
            {
                return new SpecBlock(SpecBlockKind.TaskItem, match.Groups[2].Value).SetChecked(match.Groups[1].Value != " ");
            }

            if ((match = BULLET_ITEM.Match(content)).Success) { return new SpecBlock(SpecBlockKind.BulletItem, match.Groups[1].Value); }

            if ((match = NUMBERED_ITEM.Match(content)).Success)
            {
                return new SpecBlock(SpecBlockKind.NumberedItem, match.Groups[2].Value).SetLevel(int.Parse(match.Groups[1].Value));
            }

            if ((match = QUOTE.Match(content)).Success) { return new SpecBlock(SpecBlockKind.Quote, match.Groups[1].Value); }
            if (DIVIDER.IsMatch(content)) { return new SpecBlock(SpecBlockKind.Divider, string.Empty); }

            if ((match = IMAGE.Match(content)).Success)
            {
                return new SpecBlock(SpecBlockKind.Image, match.Groups[2].Value).SetCaption(match.Groups[1].Value);
            }

            return new SpecBlock(SpecBlockKind.Paragraph, content);
        }

        /// <summary>
        ///     HTML風のタグで始まる行を読む。
        /// </summary>
        /// <param name="line">現在の行。</param>
        /// <returns>読んだブロック。タグでなく本文として扱う行なら null。</returns>
        private SpecBlock ParseTagBlock(SourceLine line)
        {
            string content = line.Content;
            Match match;

            if (content.StartsWith("<details", StringComparison.Ordinal))
            {
                string summary = string.Empty;
                if (_index < _lines.Count && _lines[_index].Content.StartsWith("<summary", StringComparison.Ordinal))
                {
                    summary = StripTags(_lines[_index].Content);
                    _index++;
                }

                SpecBlock toggle = new SpecBlock(SpecBlockKind.Toggle, string.Empty).SetCaption(summary);
                toggle.AddChildren(ParseBlocks(line.Indent, DETAILS_CLOSE));
                return toggle;
            }

            if (content.StartsWith("<callout", StringComparison.Ordinal))
            {
                string icon = ReadAttribute(content, "icon");
                SpecBlock callout = new SpecBlock(SpecBlockKind.Callout, string.Empty).SetCaption(icon);

                // 1行で閉じるコールアウトは、その行の中身を本文にする。
                int close = content.IndexOf(CALLOUT_CLOSE, StringComparison.Ordinal);
                if (close >= 0)
                {
                    string inner = content.Substring(content.IndexOf('>') + 1, close - content.IndexOf('>') - 1);
                    callout.AddChildren(new[] { new SpecBlock(SpecBlockKind.Paragraph, inner) });
                    return callout;
                }

                callout.AddChildren(ParseBlocks(line.Indent, CALLOUT_CLOSE));
                return callout;
            }

            if (content.StartsWith("<columns", StringComparison.Ordinal))
            {
                SpecBlock columns = new(SpecBlockKind.Columns, string.Empty);
                columns.AddChildren(ParseBlocks(line.Indent, COLUMNS_CLOSE));
                return columns;
            }

            if (content.StartsWith("<column", StringComparison.Ordinal))
            {
                SpecBlock column = new SpecBlock(SpecBlockKind.Column, string.Empty).SetCaption(ReadAttribute(content, "ratio"));
                column.AddChildren(ParseBlocks(line.Indent, COLUMN_CLOSE));
                return column;
            }

            if (content.StartsWith("<table_of_contents", StringComparison.Ordinal)) { return new SpecBlock(SpecBlockKind.TableOfContents, string.Empty); }
            if (content.StartsWith("<table", StringComparison.Ordinal)) { return ParseHtmlTable(line); }
            if (content.StartsWith("<empty-block", StringComparison.Ordinal)) { return new SpecBlock(SpecBlockKind.Empty, string.Empty); }

            if ((match = MEDIA.Match(content)).Success)
            {
                return new SpecBlock(SpecBlockKind.Media, match.Groups[2].Value).SetCaption(match.Groups[1].Value);
            }

            if ((match = NOTION_ONLY.Match(content)).Success)
            {
                return new SpecBlock(SpecBlockKind.NotionOnly, match.Groups[1].Value).SetCaption(match.Groups[2].Value);
            }

            return null;
        }

        /// <summary>
        ///     コードブロックを閉じるまで読む。
        /// </summary>
        /// <param name="opening">開始の行。</param>
        /// <returns>コードブロック。</returns>
        private SpecBlock ParseCode(SourceLine opening)
        {
            string language = opening.Content.Substring(3).Trim();
            StringBuilder builder = new();
            while (_index < _lines.Count)
            {
                SourceLine line = _lines[_index];
                _index++;
                if (line.Content.TrimStart().StartsWith("```", StringComparison.Ordinal)) { break; }

                if (builder.Length > 0) { builder.Append('\n'); }
                builder.Append(line.RemoveIndent(opening.Indent));
            }

            return new SpecBlock(SpecBlockKind.Code, builder.ToString()).SetCaption(language);
        }

        /// <summary>
        ///     Markdownの表（| 区切り）を読む。
        /// </summary>
        /// <param name="first">表の1行目。</param>
        /// <returns>表。</returns>
        private SpecBlock ParsePipeTable(SourceLine first)
        {
            SpecBlock table = new(SpecBlockKind.Table, string.Empty);
            List<string[]> rows = new() { SplitPipeRow(first.Content) };
            while (_index < _lines.Count && _lines[_index].Indent == first.Indent &&
                   _lines[_index].Content.StartsWith("|", StringComparison.Ordinal))
            {
                rows.Add(SplitPipeRow(_lines[_index].Content));
                _index++;
            }

            // 2行目が区切り行（|---|）なら、1行目が見出し行。
            if (rows.Count > 1 && Array.TrueForAll(rows[1], cell => TABLE_SEPARATOR_CELL.IsMatch(cell)))
            {
                rows.RemoveAt(1);
                table.MarkHeaderRow();
            }

            foreach (string[] row in rows) { table.AddRow(row); }
            return table;
        }

        /// <summary>
        ///     Notionの table タグの表を閉じるまで読む。
        /// </summary>
        /// <param name="opening">開始の行。</param>
        /// <returns>表。</returns>
        private SpecBlock ParseHtmlTable(SourceLine opening)
        {
            SpecBlock table = new(SpecBlockKind.Table, string.Empty);
            if (ReadAttribute(opening.Content, "header-row") == "true") { table.MarkHeaderRow(); }

            List<string> row = null;
            StringBuilder cell = null;
            while (_index < _lines.Count)
            {
                string content = _lines[_index].Content;
                _index++;
                if (content.StartsWith(TABLE_CLOSE, StringComparison.Ordinal)) { break; }

                // セルの中身が複数行にわたるときは、閉じるまでつなぐ。
                if (cell != null)
                {
                    cell.Append("<br>").Append(content);
                    if (content.Contains(CELL_CLOSE)) { row?.Add(StripCell(cell.ToString())); cell = null; }
                    continue;
                }

                if (content.StartsWith("<tr", StringComparison.Ordinal)) { row = new List<string>(); }
                else if (content.StartsWith("</tr", StringComparison.Ordinal))
                {
                    if (row != null) { table.AddRow(row); }
                    row = null;
                }
                else if (content.StartsWith("<td", StringComparison.Ordinal))
                {
                    if (content.Contains(CELL_CLOSE)) { row?.Add(StripCell(content)); }
                    else { cell = new StringBuilder(content); }
                }
            }

            return table;
        }

        /// <summary>
        ///     表の1行を | で区切る。\| は区切りにしない。
        /// </summary>
        /// <param name="content">行。</param>
        /// <returns>セル。</returns>
        private static string[] SplitPipeRow(string content)
        {
            string trimmed = content.Trim();
            if (trimmed.StartsWith("|", StringComparison.Ordinal)) { trimmed = trimmed.Substring(1); }
            if (trimmed.EndsWith("|", StringComparison.Ordinal) && !trimmed.EndsWith("\\|", StringComparison.Ordinal))
            {
                trimmed = trimmed.Substring(0, trimmed.Length - 1);
            }

            List<string> cells = new();
            StringBuilder builder = new();
            for (int i = 0; i < trimmed.Length; i++)
            {
                if (trimmed[i] == '\\' && i + 1 < trimmed.Length && trimmed[i + 1] == '|')
                {
                    builder.Append('|');
                    i++;
                    continue;
                }

                if (trimmed[i] == '|')
                {
                    cells.Add(builder.ToString().Trim());
                    builder.Clear();
                    continue;
                }

                builder.Append(trimmed[i]);
            }

            cells.Add(builder.ToString().Trim());
            return cells.ToArray();
        }

        /// <summary>
        ///     td タグを外してセルの中身を返す。
        /// </summary>
        /// <param name="cell">td タグを含むセル。</param>
        /// <returns>セルの中身（文中の書式は残す）。</returns>
        private static string StripCell(string cell)
        {
            int start = cell.IndexOf('>') + 1;
            int end = cell.LastIndexOf(CELL_CLOSE, StringComparison.Ordinal);
            if (end < start) { end = cell.Length; }
            return cell.Substring(start, end - start).Trim();
        }

        /// <summary>
        ///     タグをすべて取り除く。
        /// </summary>
        /// <param name="text">タグを含む文字列。</param>
        /// <returns>タグを除いた文字列。</returns>
        private static string StripTags(string text)
        {
            return TAG.Replace(text, string.Empty).Trim();
        }

        /// <summary>
        ///     タグの属性の値を読む。
        /// </summary>
        /// <param name="tag">タグを含む行。</param>
        /// <param name="name">属性名。</param>
        /// <returns>値。無ければ空文字。</returns>
        private static string ReadAttribute(string tag, string name)
        {
            foreach (Match match in ATTRIBUTE.Matches(tag))
            {
                if (match.Groups[1].Value == name) { return match.Groups[2].Value; }
            }

            return string.Empty;
        }

        /// <summary>
        ///     字下げ（先頭のタブの数）と中身に分けた1行。
        /// </summary>
        private readonly struct SourceLine
        {
            /// <summary>
            ///     行を字下げと中身に分ける。
            /// </summary>
            /// <param name="raw">元の行。</param>
            public SourceLine(string raw)
            {
                Raw = raw;
                int indent = 0;
                while (indent < raw.Length && raw[indent] == '\t') { indent++; }
                Indent = indent;
                Content = raw.Substring(indent).TrimEnd();
            }

            /// <summary> 元の行。 </summary>
            public string Raw { get; }

            /// <summary> 字下げ（先頭のタブの数）。 </summary>
            public int Indent { get; }

            /// <summary> 字下げを除いた中身。 </summary>
            public string Content { get; }

            /// <summary> 空行か。 </summary>
            public bool IsBlank => Content.Length == 0;

            /// <summary>
            ///     指定した数までのタブを先頭から除く（コードブロックの中身用）。
            /// </summary>
            /// <param name="count">除くタブの数。</param>
            /// <returns>タブを除いた行。</returns>
            public string RemoveIndent(int count)
            {
                int removed = 0;
                while (removed < count && removed < Raw.Length && Raw[removed] == '\t') { removed++; }
                return Raw.Substring(removed);
            }
        }
    }
}
