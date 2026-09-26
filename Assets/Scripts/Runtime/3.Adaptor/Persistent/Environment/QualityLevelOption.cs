namespace KillChord.Runtime.Adaptor.Persistent.Environment
{
    /// <summary>
    ///     画質プリセットの選択肢1件を表す。
    /// </summary>
    public readonly struct QualityLevelOption
    {
        /// <summary>
        ///     画質プリセットの選択肢を初期化する。
        /// </summary>
        public QualityLevelOption(int qualityLevelIndex, string name)
        {
            QualityLevelIndex = qualityLevelIndex;
            Name = name;
        }

        /// <summary> QualitySettings上のレベル番号。 </summary>
        public int QualityLevelIndex { get; }

        /// <summary> 表示名。 </summary>
        public string Name { get; }
    }
}
