using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Utility;
using System;

namespace KillChord.Runtime.Domain
{
    /// <summary>
    ///     音楽の実行タイミングを計算する静的クラス。
    /// </summary>
    public static class MusicTimingCalculator
    {
        /// <summary>
        ///     実行時間を計算する。
        /// </summary>
        /// <param name="rhythmDefinition"> リズム定義。 </param>
        /// <param name="executeRequestTiming"> 実行要求タイミング。 </param>
        /// <param name="accurateBeat"> 現在の正確な拍。 </param>
        /// <returns> 計算された実行時間。 </returns>
        public static double CalculateExecutionTime(
            RhythmDefinition rhythmDefinition,
            ExecuteRequestTiming executeRequestTiming,
            double accurateBeat)
        {
            if (executeRequestTiming.Beat.Signature <= 0d
                || executeRequestTiming.Beat.Count <= 0d
                || executeRequestTiming.Beat.Count > executeRequestTiming.Beat.Signature)
            {
                throw new ArgumentOutOfRangeException(nameof(executeRequestTiming));
            }

            double currentBar = Math.Floor(accurateBeat / MusicConstants.STANDARD_BEATS_PER_BAR);
            double targetBar = currentBar + executeRequestTiming.BarFlag;
            double targetBarStartTime = targetBar * rhythmDefinition.BarLength;
            double offsetInBar = rhythmDefinition.BarLength / executeRequestTiming.Beat.Signature * (executeRequestTiming.Beat.Count - 1d);

            return targetBarStartTime + offsetInBar;
        }
    }
}
