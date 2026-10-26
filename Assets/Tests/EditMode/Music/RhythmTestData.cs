using KillChord.Runtime.Domain.InGame.Music;

namespace KillChord.Tests.EditMode.Music
{
    /// <summary>
    ///     リズム系テストで共有する定義です。BPM120（1拍0.5秒・1小節2秒）を基準にします。
    /// </summary>
    internal static class RhythmTestData
    {
        /// <summary> テスト用のBPMです。 </summary>
        public const double BPM = 120d;

        /// <summary> BPM120での1小節の秒数です。 </summary>
        public const double BAR_SECONDS = 2d;

        /// <summary>
        ///     1拍・2拍・4拍の判定範囲を持つ判定定義を生成します。
        ///     1拍のジャストは小節末尾を越えて1.05小節まで続きます。
        /// </summary>
        /// <returns> 判定定義です。 </returns>
        public static RhythmJudgmentDefinition CreateJudgmentDefinition()
        {
            return new RhythmJudgmentDefinition(new[]
            {
                new RhythmJudgmentRange(BeatType.Four, 0f, 0.375f, 0.2f, 0.3f),
                new RhythmJudgmentRange(BeatType.Two, 0.375f, 0.75f, 0.45f, 0.55f),
                new RhythmJudgmentRange(BeatType.One, 0.75f, 1f, 0.95f, 1.05f),
            });
        }
    }
}
