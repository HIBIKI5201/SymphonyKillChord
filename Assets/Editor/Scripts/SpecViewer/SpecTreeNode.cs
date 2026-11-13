namespace KillChord.Editor.SpecViewer
{
    /// <summary>
    ///     ページツリーの1項目。ページの無いフォルダの項目は <see cref="Entry"/> が null になる。
    /// </summary>
    public sealed class SpecTreeNode
    {
        /// <summary>
        ///     項目を生成する。
        /// </summary>
        /// <param name="displayName">表示名。</param>
        /// <param name="entry">対応するページ。無ければ null。</param>
        public SpecTreeNode(string displayName, SpecCatalogEntry entry)
        {
            DisplayName = displayName;
            Entry = entry;
        }

        /// <summary> 表示名。 </summary>
        public string DisplayName { get; }

        /// <summary> 対応するページ。ページの無いフォルダなら null。 </summary>
        public SpecCatalogEntry Entry { get; }
    }
}
