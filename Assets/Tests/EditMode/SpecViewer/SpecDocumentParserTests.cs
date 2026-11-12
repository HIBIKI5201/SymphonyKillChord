using KillChord.Editor.SpecViewer;
using NUnit.Framework;
using System.Collections.Generic;

namespace KillChord.Tests.EditMode.SpecViewer
{
    /// <summary>
    ///     プロジェクト ドキュメントの写し（Notion形式のMarkdown）の解析を検証します。
    /// </summary>
    public sealed class SpecDocumentParserTests
    {
        /// <summary>
        ///     見出し・箇条書き・タブで字下げした子ブロックを、木に分けることを確認します。
        /// </summary>
        [Test]
        public void Parse_NestsTabIndentedChildren()
        {
            IReadOnlyList<SpecBlock> blocks = Parse("# 概要\n- 親\n\t- 子\n\t\t孫の段落\n- 次");

            Assert.That(blocks.Count, Is.EqualTo(3));
            Assert.That(blocks[0].Kind, Is.EqualTo(SpecBlockKind.Heading));
            Assert.That(blocks[0].Level, Is.EqualTo(1));
            Assert.That(blocks[1].Kind, Is.EqualTo(SpecBlockKind.BulletItem));
            Assert.That(blocks[1].Children[0].Text, Is.EqualTo("子"));
            Assert.That(blocks[1].Children[0].Children[0].Kind, Is.EqualTo(SpecBlockKind.Paragraph));
            Assert.That(blocks[2].Text, Is.EqualTo("次"));
        }

        /// <summary>
        ///     入れ子の details タグを、見出し付きのトグルにすることを確認します。
        /// </summary>
        [Test]
        public void Parse_ReadsNestedDetailsAsToggle()
        {
            IReadOnlyList<SpecBlock> blocks = Parse(
                "<details>\n<summary>廃案</summary>\n\t<details>\n\t<summary>図</summary>\n\t\t![](a.png)\n\t</details>\n\t本文\n</details>\n後ろ");

            Assert.That(blocks.Count, Is.EqualTo(2));
            SpecBlock outer = blocks[0];
            Assert.That(outer.Kind, Is.EqualTo(SpecBlockKind.Toggle));
            Assert.That(outer.Caption, Is.EqualTo("廃案"));
            Assert.That(outer.Children[0].Kind, Is.EqualTo(SpecBlockKind.Toggle));
            Assert.That(outer.Children[0].Children[0].Kind, Is.EqualTo(SpecBlockKind.Image));
            Assert.That(outer.Children[1].Text, Is.EqualTo("本文"));
            Assert.That(blocks[1].Text, Is.EqualTo("後ろ"));
        }

        /// <summary>
        ///     toggle 属性の付いた見出しを、開閉できる見出しにして属性を除くことを確認します。
        /// </summary>
        [Test]
        public void Parse_MarksToggleHeading()
        {
            IReadOnlyList<SpecBlock> blocks = Parse("### 拍子ごとの攻撃 {toggle=\"true\"}\n\t#### 1拍子");

            Assert.That(blocks[0].IsToggle, Is.True);
            Assert.That(blocks[0].Text, Is.EqualTo("拍子ごとの攻撃"));
            Assert.That(blocks[0].Children[0].Level, Is.EqualTo(4));
        }

        /// <summary>
        ///     Notion の table タグと Markdown の表を、見出し行付きの表にすることを確認します。
        /// </summary>
        [Test]
        public void Parse_ReadsHtmlAndPipeTables()
        {
            IReadOnlyList<SpecBlock> blocks = Parse(
                "<table header-row=\"true\">\n<tr>\n<td>項目</td>\n<td>内容</td>\n</tr>\n<tr>\n<td>a</td>\n<td>b</td>\n</tr>\n</table>\n" +
                "| プロパティ | 型 |\n|---|---|\n| 名前 | title |");

            Assert.That(blocks.Count, Is.EqualTo(2));
            Assert.That(blocks[0].HasHeaderRow, Is.True);
            Assert.That(blocks[0].Rows.Count, Is.EqualTo(2));
            Assert.That(blocks[0].Rows[1][1], Is.EqualTo("b"));
            Assert.That(blocks[1].HasHeaderRow, Is.True);
            Assert.That(blocks[1].Rows.Count, Is.EqualTo(2));
            Assert.That(blocks[1].Rows[1][0], Is.EqualTo("名前"));
        }

        /// <summary>
        ///     コールアウト・段組み・添付・空ブロック・コメントを読み分けることを確認します。
        /// </summary>
        [Test]
        public void Parse_ReadsNotionBlocks()
        {
            IReadOnlyList<SpecBlock> blocks = Parse(
                "<!-- 自動生成 -->\n<callout icon=\"💡\">\n\t**ゴール**\n</callout>\n" +
                "<columns>\n\t<column ratio=\"50\">\n\t\t左\n\t</column>\n\t<column ratio=\"50\">\n\t\t右\n\t</column>\n</columns>\n" +
                "<audio src=\"../assets/x.mp3\"></audio>\n<empty-block/>\n<table_of_contents color=\"gray\"/>");

            Assert.That(blocks.Count, Is.EqualTo(5));
            Assert.That(blocks[0].Kind, Is.EqualTo(SpecBlockKind.Callout));
            Assert.That(blocks[0].Caption, Is.EqualTo("💡"));
            Assert.That(blocks[1].Kind, Is.EqualTo(SpecBlockKind.Columns));
            Assert.That(blocks[1].Children.Count, Is.EqualTo(2));
            Assert.That(blocks[1].Children[1].Children[0].Text, Is.EqualTo("右"));
            Assert.That(blocks[2].Kind, Is.EqualTo(SpecBlockKind.Media));
            Assert.That(blocks[2].Text, Is.EqualTo("../assets/x.mp3"));
            Assert.That(blocks[3].Kind, Is.EqualTo(SpecBlockKind.Empty));
            Assert.That(blocks[4].Kind, Is.EqualTo(SpecBlockKind.TableOfContents));
        }

        /// <summary>
        ///     メンションの行はタグとして捨てず、段落として残すことを確認します。
        /// </summary>
        [Test]
        public void Parse_KeepsMentionLineAsParagraph()
        {
            IReadOnlyList<SpecBlock> blocks = Parse("<mention-page url=\"https://app.notion.com/p/3457c2c6cc028042a434f043054491f9\">チュートリアル</mention-page>");

            Assert.That(blocks.Count, Is.EqualTo(1));
            Assert.That(blocks[0].Kind, Is.EqualTo(SpecBlockKind.Paragraph));
        }

        /// <summary>
        ///     テスト用に解析します。
        /// </summary>
        /// <param name="markdown">Markdown。</param>
        /// <returns>最上位のブロック。</returns>
        private static IReadOnlyList<SpecBlock> Parse(string markdown)
        {
            return new SpecDocumentParser().Parse(markdown);
        }
    }
}
