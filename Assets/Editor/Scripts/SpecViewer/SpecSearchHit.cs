namespace KillChord.Editor.SpecViewer
{
    /// <summary>
    ///     本文検索で見つかった1ページ。
    /// </summary>
    public readonly struct SpecSearchHit
    {
        /// <summary>
        ///     検索結果を生成する。
        /// </summary>
        /// <param name="entry">見つかったページ。</param>
        /// <param name="snippet">一致した行の抜粋。</param>
        public SpecSearchHit(SpecCatalogEntry entry, string snippet)
        {
            Entry = entry;
            Snippet = snippet ?? string.Empty;
        }

        /// <summary> 見つかったページ。 </summary>
        public SpecCatalogEntry Entry { get; }

        /// <summary> 一致した行の抜粋。 </summary>
        public string Snippet { get; }
    }
}
