namespace KillChord.Editor.SpecViewer
{
    /// <summary>
    ///     プロジェクト ドキュメントの写し（Notion形式のMarkdown）のブロックの種類。
    /// </summary>
    public enum SpecBlockKind
    {
        /// <summary> 段落。 </summary>
        Paragraph,

        /// <summary> 見出し。 </summary>
        Heading,

        /// <summary> 箇条書きの項目。 </summary>
        BulletItem,

        /// <summary> 番号付きリストの項目。 </summary>
        NumberedItem,

        /// <summary> チェックボックス付きの項目。 </summary>
        TaskItem,

        /// <summary> 引用。 </summary>
        Quote,

        /// <summary> コードブロック。 </summary>
        Code,

        /// <summary> 区切り線。 </summary>
        Divider,

        /// <summary> 画像。 </summary>
        Image,

        /// <summary> 表（Markdownの表と、Notionの table タグ）。 </summary>
        Table,

        /// <summary> トグル（details タグ）。 </summary>
        Toggle,

        /// <summary> コールアウト。 </summary>
        Callout,

        /// <summary> 段組み（columns タグ）。 </summary>
        Columns,

        /// <summary> 段組みの1列（column タグ）。 </summary>
        Column,

        /// <summary> 動画・音声・添付ファイル。 </summary>
        Media,

        /// <summary> 目次（table_of_contents タグ）。 </summary>
        TableOfContents,

        /// <summary> 空のブロック。 </summary>
        Empty,

        /// <summary> 写しで表せなかったNotionの要素（リンクだけを持つ）。 </summary>
        NotionOnly,
    }
}
