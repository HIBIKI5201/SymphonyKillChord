namespace KillChord.Editor.SpecViewer
{
    /// <summary>
    ///     リンクを押したときの行き先。
    /// </summary>
    public readonly struct SpecLinkTarget
    {
        /// <summary>
        ///     行き先を生成する。
        /// </summary>
        /// <param name="kind">行き先の種類。</param>
        /// <param name="value">写しのページなら相対パス、ファイルなら絶対パス、外部ならURL。</param>
        public SpecLinkTarget(TargetKind kind, string value)
        {
            Kind = kind;
            Value = value ?? string.Empty;
        }

        /// <summary> 行き先の種類。 </summary>
        public TargetKind Kind { get; }

        /// <summary> 写しのページなら相対パス、ファイルなら絶対パス、外部ならURL。 </summary>
        public string Value { get; }

        /// <summary>
        ///     行き先の種類。
        /// </summary>
        public enum TargetKind
        {
            /// <summary> 行き先が無い（ページ内の見出しへのリンクなど）。 </summary>
            None,

            /// <summary> 写しのページ。ビューアで開く。 </summary>
            Page,

            /// <summary> 写しの中のファイル（画像・動画など）。既定のアプリで開く。 </summary>
            File,

            /// <summary> 外部のURL。ブラウザで開く。 </summary>
            External,
        }
    }
}
