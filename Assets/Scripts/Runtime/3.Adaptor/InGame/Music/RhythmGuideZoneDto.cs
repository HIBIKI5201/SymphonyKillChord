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
        /// <param name="justStartNormalized"> ジャスト開始位置。 </param>
        /// <param name="justEndNormalized"> ジャスト終了位置。 </param>
        public RhythmGuideZoneDto(int beatCount, float startNormalized, float endNormalized,
            float justStartNormalized, float justEndNormalized)
        {
            BeatCount = beatCount;
            StartNormalized = startNormalized;
            EndNormalized = endNormalized;
            JustStartNormalized = justStartNormalized;
            JustEndNormalized = justEndNormalized;
        }

        /// <summary> 拍数。 </summary>
        public int BeatCount { get; }
        /// <summary> 開始位置（正規化）。 </summary>
        public float StartNormalized { get; }
        /// <summary> 終了位置（正規化）。 </summary>
        public float EndNormalized { get; }
        /// <summary> ジャスト開始位置。 </summary>
        public float JustStartNormalized { get; }
        /// <summary> ジャスト終了位置。 </summary>
        public float JustEndNormalized { get; }
    }
}
