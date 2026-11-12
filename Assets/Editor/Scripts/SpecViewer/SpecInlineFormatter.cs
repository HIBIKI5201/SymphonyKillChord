using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace KillChord.Editor.SpecViewer
{
    /// <summary>
    ///     1ブロック分の文中の書式（強調・取り消し線・コード・リンク）を、UI Toolkit のリッチテキストに変える。
    ///     Markdown の解釈は Markdig に任せ、Notion 独自のタグ（ページへのメンション・改行・色）はその前に置き換える。
    ///     リンクは <c>link</c> タグにし、リンク先は渡されたリストに積んで、その添字をリンクIDにする。
    /// </summary>
    public sealed class SpecInlineFormatter
    {
        /// <summary>
        ///     書式の変換器を生成する。
        /// </summary>
        /// <param name="linkColor">リンクの文字色（#RRGGBB）。</param>
        /// <param name="codeBackgroundColor">コードの背景色（#RRGGBBAA）。</param>
        public SpecInlineFormatter(string linkColor, string codeBackgroundColor)
        {
            _linkColor = linkColor;
            _codeBackgroundColor = codeBackgroundColor;
        }

        /// <summary>
        ///     文中の書式をリッチテキストに変える。
        /// </summary>
        /// <param name="text">Markdown の1ブロック分の本文。</param>
        /// <param name="links">リンク先を積むリスト。リンクIDはこのリストの添字になる。</param>
        /// <returns>リッチテキスト。</returns>
        public string Format(string text, List<string> links)
        {
            if (string.IsNullOrEmpty(text)) { return string.Empty; }

            MarkdownDocument document = Markdown.Parse(ReplaceNotionTags(text), PIPELINE);
            StringBuilder builder = new();
            foreach (LeafBlock leaf in document.Descendants<LeafBlock>())
            {
                if (leaf.Inline == null) { continue; }
                if (builder.Length > 0) { builder.Append('\n'); }
                AppendInlines(builder, leaf.Inline, links);
            }

            return builder.ToString();
        }

        /// <summary>
        ///     リンクや書式を除いた、表示用の素の文字列にする（トグルの見出しなど、リンクを押せない場所用）。
        /// </summary>
        /// <param name="text">Markdown の1ブロック分の本文。</param>
        /// <returns>素の文字列。</returns>
        public string ToPlainText(string text)
        {
            if (string.IsNullOrEmpty(text)) { return string.Empty; }

            MarkdownDocument document = Markdown.Parse(ReplaceNotionTags(text), PIPELINE);
            StringBuilder builder = new();
            foreach (LeafBlock leaf in document.Descendants<LeafBlock>())
            {
                if (leaf.Inline == null) { continue; }
                foreach (LiteralInline literal in leaf.Inline.FindDescendants<LiteralInline>())
                {
                    builder.Append(literal.Content.ToString());
                }
            }

            return builder.ToString();
        }

        /// <summary>
        ///     リッチテキストのタグとして解釈されないよう、文字列を囲む。
        /// </summary>
        /// <param name="text">そのまま表示したい文字列。</param>
        /// <returns>タグとして解釈されない文字列。</returns>
        public static string Escape(string text)
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf('<') < 0) { return text ?? string.Empty; }

            return $"{NOPARSE_OPEN}{text.Replace(NOPARSE_CLOSE, "</ noparse>")}{NOPARSE_CLOSE}";
        }

        private const string NOPARSE_OPEN = "<noparse>";
        private const string NOPARSE_CLOSE = "</noparse>";
        private const int STRONG_DELIMITER_COUNT = 2;

        private static readonly MarkdownPipeline PIPELINE = new MarkdownPipelineBuilder()
            .UseEmphasisExtras()
            .UseAutoLinks()
            .Build();

        private static readonly Regex MENTION = new(@"<mention-[a-z]+\s+url=""([^""]*)""[^>]*>(.*?)</mention-[a-z]+>");
        private static readonly Regex SELF_CLOSING_MENTION = new(@"<mention-[a-z]+\s+url=""([^""]*)""[^>]*/>");
        private static readonly Regex LINE_BREAK = new(@"<br\s*/?>");
        private static readonly Regex DECORATION_TAG = new(@"</?(span|u|color)[^>]*>");

        private readonly string _linkColor;
        private readonly string _codeBackgroundColor;

        /// <summary>
        ///     Notion 独自のタグを、Markdown の書き方に置き換える。
        /// </summary>
        /// <param name="text">元の本文。</param>
        /// <returns>置き換えた本文。</returns>
        private static string ReplaceNotionTags(string text)
        {
            // ページ・DBへのメンションはリンクにする。
            string replaced = MENTION.Replace(text, match => $"[{match.Groups[2].Value}](<{match.Groups[1].Value}>)");
            replaced = SELF_CLOSING_MENTION.Replace(replaced, match => $"[↗](<{match.Groups[1].Value}>)");

            // 改行は、段落の中の改行として扱う。
            replaced = LINE_BREAK.Replace(replaced, "  \n");

            // 色・下線・コメントの印は表示しない。
            return DECORATION_TAG.Replace(replaced, string.Empty);
        }

        /// <summary>
        ///     文中の要素を順にリッチテキストへ書き出す。
        /// </summary>
        /// <param name="builder">書き出し先。</param>
        /// <param name="container">文中の要素の親。</param>
        /// <param name="links">リンク先を積むリスト。</param>
        private void AppendInlines(StringBuilder builder, ContainerInline container, List<string> links)
        {
            foreach (Inline inline in container)
            {
                AppendInline(builder, inline, links);
            }
        }

        /// <summary>
        ///     文中の要素を1つリッチテキストへ書き出す。
        /// </summary>
        /// <param name="builder">書き出し先。</param>
        /// <param name="inline">文中の要素。</param>
        /// <param name="links">リンク先を積むリスト。</param>
        private void AppendInline(StringBuilder builder, Inline inline, List<string> links)
        {
            switch (inline)
            {
                case LiteralInline literal:
                    builder.Append(Escape(literal.Content.ToString()));
                    break;
                case CodeInline code:
                    builder.Append($"<mark={_codeBackgroundColor}>").Append(Escape(code.Content)).Append("</mark>");
                    break;
                case LineBreakInline:
                    builder.Append('\n');
                    break;
                case HtmlEntityInline entity:
                    builder.Append(Escape(entity.Transcoded.ToString()));
                    break;
                case AutolinkInline autolink:
                    AppendLink(builder, autolink.Url, Escape(autolink.Url), links);
                    break;
                case LinkInline link:
                    AppendLinkInline(builder, link, links);
                    break;
                case EmphasisInline emphasis:
                    AppendEmphasis(builder, emphasis, links);
                    break;
                case ContainerInline nested:
                    AppendInlines(builder, nested, links);
                    break;
            }
        }

        /// <summary>
        ///     リンク（画像の埋め込みを含む）を書き出す。
        /// </summary>
        /// <param name="builder">書き出し先。</param>
        /// <param name="link">リンク。</param>
        /// <param name="links">リンク先を積むリスト。</param>
        private void AppendLinkInline(StringBuilder builder, LinkInline link, List<string> links)
        {
            StringBuilder label = new();
            AppendInlines(label, link, links);
            if (link.IsImage) { label.Insert(0, "画像: "); }
            if (label.Length == 0) { label.Append(Escape(link.Url)); }
            AppendLink(builder, link.Url, label.ToString(), links);
        }

        /// <summary>
        ///     リンクタグを書き出す。
        /// </summary>
        /// <param name="builder">書き出し先。</param>
        /// <param name="url">リンク先。</param>
        /// <param name="label">表示する文字列（リッチテキスト）。</param>
        /// <param name="links">リンク先を積むリスト。</param>
        private void AppendLink(StringBuilder builder, string url, string label, List<string> links)
        {
            int id = links.Count;
            links.Add(url ?? string.Empty);
            builder.Append($"<link=\"{id}\"><color={_linkColor}><u>").Append(label).Append("</u></color></link>");
        }

        /// <summary>
        ///     強調・取り消し線を書き出す。
        /// </summary>
        /// <param name="builder">書き出し先。</param>
        /// <param name="emphasis">強調。</param>
        /// <param name="links">リンク先を積むリスト。</param>
        private void AppendEmphasis(StringBuilder builder, EmphasisInline emphasis, List<string> links)
        {
            string tag = emphasis.DelimiterChar == '~'
                ? "s"
                : emphasis.DelimiterCount >= STRONG_DELIMITER_COUNT ? "b" : "i";
            builder.Append('<').Append(tag).Append('>');
            AppendInlines(builder, emphasis, links);
            builder.Append("</").Append(tag).Append('>');
        }
    }
}
