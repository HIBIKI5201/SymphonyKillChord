using KillChord.Editor.SpecViewer;
using NUnit.Framework;

namespace KillChord.Tests.EditMode.SpecViewer
{
    /// <summary>
    ///     写しのページのリンクの行き先の決め方を検証します。
    /// </summary>
    public sealed class SpecLinkResolverTests
    {
        /// <summary>
        ///     % エンコードされた相対リンクを、写しのページとして開くことを確認します。
        /// </summary>
        [Test]
        public void Resolve_OpensRelativeMarkdownLinkInViewer()
        {
            SpecLinkTarget target = CreateResolver().Resolve(
                "Symphony Kill Chord.md",
                "Symphony%20Kill%20Chord/%E5%9F%BA%E6%9C%AC%E6%83%85%E5%A0%B1.md");

            Assert.That(target.Kind, Is.EqualTo(SpecLinkTarget.TargetKind.Page));
            Assert.That(target.Value, Is.EqualTo("Symphony Kill Chord/基本情報.md"));
        }

        /// <summary>
        ///     写しにあるページを指す Notion のURLは、ビューアで開くことを確認します。
        /// </summary>
        [Test]
        public void Resolve_OpensKnownNotionUrlInViewer()
        {
            SpecLinkTarget target = CreateResolver().Resolve(
                "Symphony Kill Chord.md",
                "https://app.notion.com/p/Tutorial-3457c2c6cc028042a434f043054491f9#3ff7c2c6cc028114b228e12f22d75a41");

            Assert.That(target.Kind, Is.EqualTo(SpecLinkTarget.TargetKind.Page));
            Assert.That(target.Value, Is.EqualTo("Symphony Kill Chord/仕様概要/チュートリアル.md"));
        }

        /// <summary>
        ///     写しに無いページ・外部サイトはブラウザ、写しの画像はファイルとして開くことを確認します。
        /// </summary>
        [Test]
        public void Resolve_SeparatesExternalAndFileLinks()
        {
            SpecLinkResolver resolver = CreateResolver();

            Assert.That(resolver.Resolve("a.md", "https://app.notion.com/p/00000000000000000000000000000000").Kind,
                Is.EqualTo(SpecLinkTarget.TargetKind.External));
            Assert.That(resolver.Resolve("a.md", "https://example.com/").Kind, Is.EqualTo(SpecLinkTarget.TargetKind.External));
            Assert.That(resolver.Resolve("Symphony Kill Chord/仕様概要/チュートリアル.md", "../../assets/x/001-image.png").Kind,
                Is.EqualTo(SpecLinkTarget.TargetKind.File));
            Assert.That(resolver.Resolve("a.md", "#見出し").Kind, Is.EqualTo(SpecLinkTarget.TargetKind.None));
        }

        /// <summary>
        ///     .. で写しの外へ出る相対リンクは、行き先なしにすることを確認します。
        /// </summary>
        [Test]
        public void CombineRelative_RejectsPathOutsideMirror()
        {
            Assert.That(SpecLinkResolver.CombineRelative("a/b.md", "../c.md"), Is.EqualTo("c.md"));
            Assert.That(SpecLinkResolver.CombineRelative("a/b.md", "../../c.md"), Is.Null);
        }

        /// <summary>
        ///     テスト用の解決器を作ります。
        /// </summary>
        /// <returns>解決器。</returns>
        private static SpecLinkResolver CreateResolver()
        {
            SpecCatalog catalog = new(new[]
            {
                new SpecCatalogEntry("Symphony Kill Chord.md", "Symphony Kill Chord", "27d7c2c6cc02801d9648fbe2769f1971", null, null, null, null, null),
                new SpecCatalogEntry("Symphony Kill Chord/基本情報.md", "基本情報", null, null, null, null, null, null),
                new SpecCatalogEntry("Symphony Kill Chord/仕様概要/チュートリアル.md", "チュートリアル", "3457c2c6-cc02-8042-a434-f043054491f9", null, null, null, null, null),
            }, string.Empty);
            return new SpecLinkResolver(catalog, "/mirror");
        }
    }
}
