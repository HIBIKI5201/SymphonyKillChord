using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace KillChord.Editor.SpecViewer
{
    /// <summary>
    ///     ブロックの木を UI Toolkit の要素にする。
    ///     Notion 独自のブロック（トグル・コールアウト・段組み・目次・メンション）もここで表す。
    ///     リンク・画像・添付ファイルを押したときの処理と、画像の読み込みは呼び出し元に任せる。
    /// </summary>
    public sealed class SpecDocumentRenderer
    {
        /// <summary>
        ///     描画器を生成する。
        /// </summary>
        /// <param name="formatter">文中の書式の変換器。</param>
        /// <param name="linkClicked">リンク・画像・添付ファイルが押されたときの処理（引数はページに書かれたリンク先）。</param>
        /// <param name="imageLoader">ページに書かれた画像のパスから画像を読む処理。読めなければ null を返す。</param>
        /// <param name="scrollRequested">目次から見出しへ移るときの処理。</param>
        public SpecDocumentRenderer(
            SpecInlineFormatter formatter,
            Action<string> linkClicked,
            Func<string, Texture2D> imageLoader,
            Action<VisualElement> scrollRequested)
        {
            _formatter = formatter;
            _linkClicked = linkClicked;
            _imageLoader = imageLoader;
            _scrollRequested = scrollRequested;
        }

        /// <summary>
        ///     ブロックの木を要素にする。
        /// </summary>
        /// <param name="blocks">最上位のブロック。</param>
        /// <returns>ページ全体の要素。</returns>
        public VisualElement Render(IReadOnlyList<SpecBlock> blocks)
        {
            _headings.Clear();
            _tableOfContents.Clear();
            VisualElement page = new();
            AddBlocks(page, blocks);

            // 目次は、全部の見出しを描いてから埋める。
            foreach (VisualElement container in _tableOfContents) { FillTableOfContents(container); }
            return page;
        }

        private const int MAX_TABLE_ROWS = 300;
        private const float MIN_TABLE_CELL_WIDTH = 60f;
        private const float MAX_IMAGE_WIDTH = 640f;
        private const float BLOCK_MARGIN = 3f;
        private const float CHILD_INDENT = 16f;
        private const float MARKER_WIDTH = 22f;
        private const float BORDER_WIDTH = 1f;
        private const float BOX_PADDING = 6f;
        private const float EMPTY_BLOCK_HEIGHT = 8f;
        private const int BASE_FONT_SIZE = 13;
        private static readonly int[] HEADING_FONT_SIZES = { 22, 18, 15, 14, 13, 13 };

        private readonly SpecInlineFormatter _formatter;
        private readonly Action<string> _linkClicked;
        private readonly Func<string, Texture2D> _imageLoader;
        private readonly Action<VisualElement> _scrollRequested;
        private readonly List<(int Level, string Text, VisualElement Element)> _headings = new();
        private readonly List<VisualElement> _tableOfContents = new();

        /// <summary>
        ///     ブロックを順に親へ足す。
        /// </summary>
        /// <param name="parent">親の要素。</param>
        /// <param name="blocks">ブロック。</param>
        private void AddBlocks(VisualElement parent, IReadOnlyList<SpecBlock> blocks)
        {
            foreach (SpecBlock block in blocks)
            {
                VisualElement element = RenderBlock(block);
                if (element != null) { parent.Add(element); }
            }
        }

        /// <summary>
        ///     1ブロックを要素にする。
        /// </summary>
        /// <param name="block">ブロック。</param>
        /// <returns>要素。表示しないブロックなら null。</returns>
        private VisualElement RenderBlock(SpecBlock block)
        {
            switch (block.Kind)
            {
                case SpecBlockKind.Heading: return RenderHeading(block);
                case SpecBlockKind.BulletItem: return RenderListItem(block, "•");
                case SpecBlockKind.NumberedItem: return RenderListItem(block, $"{block.Level}.");
                case SpecBlockKind.TaskItem: return RenderListItem(block, block.IsChecked ? "☑" : "☐");
                case SpecBlockKind.Quote: return RenderBox(block, CreateRichLabel(block.Text), SpecViewerStyles.QuoteBackground);
                case SpecBlockKind.Callout: return RenderCallout(block);
                case SpecBlockKind.Code: return RenderCode(block);
                case SpecBlockKind.Divider: return RenderDivider();
                case SpecBlockKind.Image: return RenderImage(block);
                case SpecBlockKind.Table: return SpecTableRenderer.Render(block, CreateRichLabel, MAX_TABLE_ROWS, MIN_TABLE_CELL_WIDTH);
                case SpecBlockKind.Toggle: return RenderToggle(_formatter.ToPlainText(block.Caption), block.Children, BASE_FONT_SIZE);
                case SpecBlockKind.Columns: return RenderColumns(block);
                case SpecBlockKind.Column: return RenderColumn(block);
                case SpecBlockKind.Media: return RenderMedia(block);
                case SpecBlockKind.NotionOnly: return RenderNotionOnly(block);
                case SpecBlockKind.TableOfContents: return RenderTableOfContentsPlaceholder();
                case SpecBlockKind.Empty: return new VisualElement { style = { height = EMPTY_BLOCK_HEIGHT } };
                default: return RenderWithChildren(CreateRichLabel(block.Text), block.Children);
            }
        }

        /// <summary>
        ///     見出しを描く。トグル見出しは開閉できるようにする。
        /// </summary>
        /// <param name="block">見出しのブロック。</param>
        /// <returns>要素。</returns>
        private VisualElement RenderHeading(SpecBlock block)
        {
            int fontSize = HEADING_FONT_SIZES[Mathf.Clamp(block.Level - 1, 0, HEADING_FONT_SIZES.Length - 1)];
            string plain = _formatter.ToPlainText(block.Text);
            VisualElement element = block.IsToggle
                ? RenderToggle(plain, block.Children, fontSize)
                : RenderWithChildren(CreateRichLabel(block.Text), block.Children);
            element.style.marginTop = fontSize / 2f;
            if (!block.IsToggle)
            {
                Label label = element.Q<Label>();
                label.style.fontSize = fontSize;
                label.style.unityFontStyleAndWeight = FontStyle.Bold;
            }

            _headings.Add((block.Level, plain, element));
            return element;
        }

        /// <summary>
        ///     箇条書き・番号付き・チェックボックスの項目を描く。
        /// </summary>
        /// <param name="block">項目のブロック。</param>
        /// <param name="marker">先頭の記号。</param>
        /// <returns>要素。</returns>
        private VisualElement RenderListItem(SpecBlock block, string marker)
        {
            VisualElement row = new() { style = { flexDirection = FlexDirection.Row, marginTop = BLOCK_MARGIN / 2f } };
            row.Add(new Label(marker) { style = { width = MARKER_WIDTH, unityTextAlign = TextAnchor.UpperRight, paddingRight = 4 } });
            VisualElement body = RenderWithChildren(CreateRichLabel(block.Text), block.Children);
            body.style.flexGrow = 1;
            body.style.flexShrink = 1;
            row.Add(body);
            return row;
        }

        /// <summary>
        ///     要素の下に、字下げした子ブロックを足す。
        /// </summary>
        /// <param name="element">本体の要素。</param>
        /// <param name="children">子ブロック。</param>
        /// <returns>本体と子ブロックをまとめた要素。</returns>
        private VisualElement RenderWithChildren(VisualElement element, IReadOnlyList<SpecBlock> children)
        {
            if (children.Count == 0) { return element; }

            VisualElement wrapper = new();
            wrapper.Add(element);
            VisualElement nested = new() { style = { marginLeft = CHILD_INDENT } };
            AddBlocks(nested, children);
            wrapper.Add(nested);
            return wrapper;
        }

        /// <summary>
        ///     開閉できる要素を描く。中身は初めて開いたときに描く（画像の多いページを軽くするため）。
        /// </summary>
        /// <param name="title">見出し。</param>
        /// <param name="children">中身のブロック。</param>
        /// <param name="fontSize">見出しの文字の大きさ。</param>
        /// <returns>要素。</returns>
        private VisualElement RenderToggle(string title, IReadOnlyList<SpecBlock> children, int fontSize)
        {
            Foldout foldout = new() { text = title, value = false, style = { marginTop = BLOCK_MARGIN } };
            foldout.Q<Toggle>().style.fontSize = fontSize;
            foldout.Q<Toggle>().style.unityFontStyleAndWeight = FontStyle.Bold;
            bool isRendered = false;
            foldout.RegisterValueChangedCallback(evt =>
            {
                // 入れ子のトグルの変更も伝わってくるので、自分の変更だけを見る。
                if (evt.target != foldout || !evt.newValue || isRendered) { return; }
                isRendered = true;
                AddBlocks(foldout.contentContainer, children);
            });
            return foldout;
        }

        /// <summary>
        ///     枠で囲んだ要素（引用・コールアウト）を描く。
        /// </summary>
        /// <param name="block">ブロック。</param>
        /// <param name="content">枠の中の先頭の要素。</param>
        /// <param name="background">背景色。</param>
        /// <returns>要素。</returns>
        private VisualElement RenderBox(SpecBlock block, VisualElement content, Color background)
        {
            VisualElement box = new()
            {
                style =
                {
                    backgroundColor = background,
                    borderLeftWidth = BORDER_WIDTH * 3, borderLeftColor = SpecViewerStyles.BorderColor,
                    paddingLeft = BOX_PADDING, paddingRight = BOX_PADDING, paddingTop = BOX_PADDING, paddingBottom = BOX_PADDING,
                    marginTop = BLOCK_MARGIN, marginBottom = BLOCK_MARGIN,
                }
            };
            if (content != null) { box.Add(content); }
            AddBlocks(box, block.Children);
            return box;
        }

        /// <summary>
        ///     コールアウトを描く。絵文字は見出しとして先頭に出す。
        /// </summary>
        /// <param name="block">コールアウトのブロック。</param>
        /// <returns>要素。</returns>
        private VisualElement RenderCallout(SpecBlock block)
        {
            Label icon = string.IsNullOrEmpty(block.Caption) ? null : new Label(block.Caption);
            return RenderBox(block, icon, SpecViewerStyles.CalloutBackground);
        }

        /// <summary>
        ///     コードブロックを描く。
        /// </summary>
        /// <param name="block">コードのブロック。</param>
        /// <returns>要素。</returns>
        private static VisualElement RenderCode(SpecBlock block)
        {
            Label code = new(SpecInlineFormatter.Escape(block.Text))
            {
                style =
                {
                    backgroundColor = SpecViewerStyles.CodeBackground,
                    paddingLeft = BOX_PADDING, paddingRight = BOX_PADDING, paddingTop = BOX_PADDING, paddingBottom = BOX_PADDING,
                    marginTop = BLOCK_MARGIN, marginBottom = BLOCK_MARGIN,
                    whiteSpace = WhiteSpace.Normal,
                }
            };
            SpecViewerStyles.ApplyMonospaceFont(code);
            return code;
        }

        /// <summary>
        ///     区切り線を描く。
        /// </summary>
        /// <returns>要素。</returns>
        private static VisualElement RenderDivider()
        {
            return new VisualElement
            {
                style = { height = BORDER_WIDTH, backgroundColor = SpecViewerStyles.BorderColor, marginTop = BOX_PADDING, marginBottom = BOX_PADDING }
            };
        }

        /// <summary>
        ///     画像を描く。読めない形式（webp など）はリンクにする。押すと既定のアプリで開く。
        /// </summary>
        /// <param name="block">画像のブロック。</param>
        /// <returns>要素。</returns>
        private VisualElement RenderImage(SpecBlock block)
        {
            Texture2D texture = _imageLoader(block.Text);
            if (texture == null) { return RenderFileLink(block.Text, $"画像を開く: {DecodeFileName(block.Text)}"); }

            float width = Mathf.Min(texture.width, MAX_IMAGE_WIDTH);
            Image image = new()
            {
                image = texture,
                scaleMode = ScaleMode.ScaleToFit,
                tooltip = "クリックすると既定のアプリで開きます",
                style = { width = width, height = width * texture.height / texture.width, marginTop = BLOCK_MARGIN, marginBottom = BLOCK_MARGIN, maxWidth = Length.Percent(100) }
            };
            string href = block.Text;
            image.RegisterCallback<ClickEvent>(_ => _linkClicked(href));
            return image;
        }

        /// <summary>
        ///     段組みを描く。列の比率は Notion の ratio に合わせる。
        /// </summary>
        /// <param name="block">段組みのブロック。</param>
        /// <returns>要素。</returns>
        private VisualElement RenderColumns(SpecBlock block)
        {
            VisualElement row = new() { style = { flexDirection = FlexDirection.Row } };
            foreach (SpecBlock column in block.Children)
            {
                VisualElement element = RenderBlock(column);
                if (element == null) { continue; }

                element.style.flexGrow = float.TryParse(column.Caption, out float ratio) && ratio > 0 ? ratio : 1f;
                element.style.flexBasis = 0;
                element.style.paddingRight = BOX_PADDING;
                row.Add(element);
            }

            return row;
        }

        /// <summary>
        ///     段組みの1列を描く。
        /// </summary>
        /// <param name="block">列のブロック。</param>
        /// <returns>要素。</returns>
        private VisualElement RenderColumn(SpecBlock block)
        {
            VisualElement column = new();
            AddBlocks(column, block.Children);
            return column;
        }

        /// <summary>
        ///     動画・音声・添付ファイルを、既定のアプリで開くリンクとして描く。
        /// </summary>
        /// <param name="block">ブロック。</param>
        /// <returns>要素。</returns>
        private VisualElement RenderMedia(SpecBlock block)
        {
            string kind = block.Caption switch { "video" => "動画", "audio" => "音声", _ => "ファイル" };
            return RenderFileLink(block.Text, $"{kind}を開く: {DecodeFileName(block.Text)}");
        }

        /// <summary>
        ///     写しで表せなかった Notion の要素を、Notion で開くリンクとして描く。
        /// </summary>
        /// <param name="block">ブロック。</param>
        /// <returns>要素。</returns>
        private VisualElement RenderNotionOnly(SpecBlock block)
        {
            // 別ページへのエイリアスやブックマークは写しに中身が無いので、目立たない小さなリンクにする。
            string kind = block.Caption switch { "alias" => "ページへのリンク", "bookmark" => "ブックマーク", _ => "Notion の要素" };
            return CreateRichLabel($"[{kind}（写しでは表示できません。Notion で開く）](<{block.Text}>)");
        }

        /// <summary>
        ///     押すとリンク先を開くボタン風のラベルを描く。
        /// </summary>
        /// <param name="href">リンク先。</param>
        /// <param name="text">表示する文字列。</param>
        /// <returns>要素。</returns>
        private VisualElement RenderFileLink(string href, string text)
        {
            Button button = new(() => _linkClicked(href))
            {
                text = text,
                style = { alignSelf = Align.FlexStart, marginTop = BLOCK_MARGIN, marginBottom = BLOCK_MARGIN, whiteSpace = WhiteSpace.Normal }
            };
            return button;
        }

        /// <summary>
        ///     目次の置き場所を作る。中身は全部の見出しを描いた後で埋める。
        /// </summary>
        /// <returns>要素。</returns>
        private VisualElement RenderTableOfContentsPlaceholder()
        {
            VisualElement container = new() { style = { marginTop = BLOCK_MARGIN, marginBottom = BLOCK_MARGIN } };
            _tableOfContents.Add(container);
            return container;
        }

        /// <summary>
        ///     目次を埋める。項目を押すと見出しへ移る。
        /// </summary>
        /// <param name="container">目次の置き場所。</param>
        private void FillTableOfContents(VisualElement container)
        {
            foreach ((int level, string text, VisualElement element) in _headings)
            {
                VisualElement target = element;
                Label item = new($"<color={SpecViewerStyles.LinkColorHex}>{SpecInlineFormatter.Escape(text)}</color>")
                {
                    style = { marginLeft = (level - 1) * CHILD_INDENT, whiteSpace = WhiteSpace.Normal }
                };
                item.RegisterCallback<ClickEvent>(_ => _scrollRequested(target));
                container.Add(item);
            }
        }

        /// <summary>
        ///     文中の書式を含むラベルを作る。リンクを押すと呼び出し元へ伝える。
        /// </summary>
        /// <param name="text">Markdown の本文。</param>
        /// <returns>ラベル。</returns>
        private Label CreateRichLabel(string text)
        {
            List<string> links = new();
            Label label = new(_formatter.Format(text, links))
            {
                enableRichText = true,
                style = { whiteSpace = WhiteSpace.Normal, marginTop = BLOCK_MARGIN / 2f, fontSize = BASE_FONT_SIZE }
            };
            if (links.Count == 0) { return label; }

            label.RegisterCallback<PointerUpLinkTagEvent>(evt =>
            {
                if (int.TryParse(evt.linkID, out int index) && index >= 0 && index < links.Count) { _linkClicked(links[index]); }
            });
            return label;
        }

        /// <summary>
        ///     パスの最後のファイル名を、% エンコードを戻して返す。
        /// </summary>
        /// <param name="href">パス。</param>
        /// <returns>ファイル名。</returns>
        private static string DecodeFileName(string href)
        {
            string name = href.Substring(href.LastIndexOf('/') + 1);
            try
            {
                return Uri.UnescapeDataString(name);
            }
            catch (UriFormatException)
            {
                return name;
            }
        }
    }
}
