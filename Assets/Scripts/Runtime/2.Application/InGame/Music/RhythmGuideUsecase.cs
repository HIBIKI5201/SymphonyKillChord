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

        /// <summary>
        ///     インジケーターの正規化された位置を計算する。
        /// </summary>
        /// <param name="barProgress"> 小節内の進捗。 </param>
        /// <returns> 正規化された位置。 </returns>
        public float CalculateIndicatorNormalized(float barProgress)
        {
            if (barProgress <= 0f) return 0f;
            if (barProgress >= 1f) return 1f;

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
