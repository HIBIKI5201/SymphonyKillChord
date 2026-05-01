using KillChord.Runtime.Domain.InGame.Music;
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

            double currentBar = Math.Floor(accurateBeat / FOUR_FOUR_BEAT_COUNT);
            double targetBar = currentBar + executeRequestTiming.BarFlag;
            double targetBarStartTime = targetBar * rhythmDefinition.BarLength;
            double offsetInBar = rhythmDefinition.BarLength / executeRequestTiming.Beat.Signature * (executeRequestTiming.Beat.Count - 1d);

            return targetBarStartTime + offsetInBar;
        }

        private const double FOUR_FOUR_BEAT_COUNT = 4d;
    }
}
