using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Domain.InGame.Music
{
    /// <summary>
    ///     リズムガイドの定義（判定範囲の集合）を保持するクラス。
    /// </summary>
    public class RhythmGuideDefinition
    {
        /// <summary>
        ///     新しい定義を生成する。
        /// </summary>
        /// <param name="guideRanges"> 判定範囲のリスト。 </param>
        public RhythmGuideDefinition(IReadOnlyList<RhythmGuideRange> guideRanges)
        {
            if (guideRanges == null) throw new ArgumentNullException(nameof(guideRanges));
            _guideRanges = guideRanges;
        }

        /// <summary> 判定範囲のリスト。 </summary>
        public IReadOnlyList<RhythmGuideRange> GuideRanges => _guideRanges;

        private readonly IReadOnlyList<RhythmGuideRange> _guideRanges;
    }
}
