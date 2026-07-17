using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Domain.InGame.Music
{
    /// <summary>
    ///     リズム判定の定義（判定範囲の集合）を保持するクラス。
    /// </summary>
    public class RhythmJudgmentDefinition
    {
        /// <summary>
        ///     新しい定義を生成する。
        /// </summary>
        /// <param name="judgmentRanges"> 判定範囲のリスト。 </param>
        public RhythmJudgmentDefinition(IReadOnlyList<RhythmJudgmentRange> judgmentRanges)
        {
            if (judgmentRanges == null)
            {
                throw new ArgumentNullException(nameof(judgmentRanges));
            }

            _judgmentRanges = judgmentRanges;
        }

        /// <summary> 判定範囲のリスト。 </summary>
        public IReadOnlyList<RhythmJudgmentRange> JudgmentRanges => _judgmentRanges;

        /// <summary>
        ///     正規化された小節進捗から拍の種類を解決する。
        /// </summary>
        /// <param name="normalizedBarProgress"> 0〜1に正規化された小節進捗。 </param>
        /// <param name="beatType"> 解決した拍の種類。 </param>
        /// <returns> 対応する判定範囲が存在する場合はtrue。 </returns>
        public bool TryResolveBeatType(float normalizedBarProgress, out BeatType beatType)
        {
            for (int i = 0; i < _judgmentRanges.Count; i++)
            {
                RhythmJudgmentRange judgmentRange = _judgmentRanges[i];
                if (judgmentRange.Contains(normalizedBarProgress))
                {
                    beatType = judgmentRange.BeatType;
                    return true;
                }
            }

            beatType = default;
            return false;
        }

        private readonly IReadOnlyList<RhythmJudgmentRange> _judgmentRanges;
    }
}
