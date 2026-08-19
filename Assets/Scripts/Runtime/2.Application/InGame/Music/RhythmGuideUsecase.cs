using KillChord.Runtime.Domain.InGame.Music;

namespace KillChord.Runtime.Application.InGame.Music
{
    /// <summary>
        ///     リズムガイドの計算ロジックを担当するユースケースクラス。
    /// </summary>
    public class RhythmGuideUsecase
    {
        /// <summary>
        ///     新しいユースケースを生成する。
        /// </summary>
        /// <param name="rhythmJudgmentDefinition"> リズム判定の定義。 </param>
        public RhythmGuideUsecase(RhythmJudgmentDefinition rhythmJudgmentDefinition)
        {
            _rhythmJudgmentDefinition = rhythmJudgmentDefinition;
        }

        /// <summary> リズム判定の定義。 </summary>
        public RhythmJudgmentDefinition RhythmJudgmentDefinition => _rhythmJudgmentDefinition;

        /// <summary> インジケーターがジャストタイミングを超えて進む量（小節基準の正規化値）。 </summary>
        public const float INDICATOR_OVERRUN_NORMALIZED = 0.05f;

        /// <summary> インジケーター位置の上限（小節基準の正規化値）。 </summary>
        public const float MAX_INDICATOR_NORMALIZED = 1f + INDICATOR_OVERRUN_NORMALIZED;

        /// <summary>
        ///     インジケーターの正規化された位置を計算する。
        /// </summary>
        /// <param name="barProgress"> 小節内の進捗。1を超える超過分も受け取る。 </param>
        /// <returns> 0〜MAX_INDICATOR_NORMALIZEDに収めた正規化位置。 </returns>
        public float CalculateIndicatorNormalized(float barProgress)
        {
            if (barProgress <= 0f)
            {
                return 0f;
            }

            // 1拍ゾーンのジャストタイミングは「1小節地点」と一致するため、1でクランプすると
            // インジケーターがジャスト位置で止まり通過して見えない。
            // 表示上ジャストを通過させるためINDICATOR_OVERRUN_NORMALIZED分だけ超過を許容する。
            if (barProgress >= MAX_INDICATOR_NORMALIZED)
            {
                return MAX_INDICATOR_NORMALIZED;
            }

            return barProgress;
        }

        /// <summary>
        ///     指定された拍における拍の種類を計算する。
        /// </summary>
        /// <param name="normalizedBarProgress"> 小節内の正規化進捗。 </param>
        /// <returns> 拍の種類。範囲外の場合は null。 </returns>
        public BeatType? CalculateCurrentBeatType(float normalizedBarProgress)
        {
            if (_rhythmJudgmentDefinition.TryResolveBeatType(normalizedBarProgress, out BeatType beatType))
            {
                return beatType;
            }

            return null;
        }

        private readonly RhythmJudgmentDefinition _rhythmJudgmentDefinition;
    }
}
