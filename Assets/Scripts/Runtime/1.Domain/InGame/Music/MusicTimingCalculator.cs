using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Utility;
using System;

namespace KillChord.Runtime.Domain
{
    public static class MusicTimingCalculator
    {
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
