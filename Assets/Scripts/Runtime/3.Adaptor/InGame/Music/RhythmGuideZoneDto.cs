namespace KillChord.Runtime.Adaptor.InGame.Music
{
    /// <summary>
    ///     リズムガイドの判定ゾーン情報を保持するDTO。
    /// </summary>
    public readonly struct RhythmGuideZoneDto
    {
        /// <summary>
        ///     新しい判定ゾーンDTOを生成する。
        /// </summary>
        /// <param name="beatCount"> 拍数。 </param>
        /// <param name="startNormalized"> 開始位置（正規化）。 </param>
        /// <param name="endNormalized"> 終了位置（正規化）。 </param>
        public RhythmGuideZoneDto(int beatCount, float startNormalized, float endNormalized)
        {
            BeatCount = beatCount;
            StartNormalized = startNormalized;
            EndNormalized = endNormalized;
        }

        /// <summary> 拍数。 </summary>
        public int BeatCount { get; }
        /// <summary> 開始位置（正規化）。 </summary>
        public float StartNormalized { get; }
        /// <summary> 終了位置（正規化）。 </summary>
        public float EndNormalized { get; }
    }
}
