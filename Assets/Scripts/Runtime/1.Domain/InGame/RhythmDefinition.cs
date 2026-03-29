using System;

namespace KillChord.Runtime.Domain
{
    public readonly struct RhythmDefinition
    {
        public readonly int Bpm;
        public readonly double BeatLength;

        public RhythmDefinition(int bpm)
        {
            if (bpm <= 0) throw new ArgumentOutOfRangeException(nameof(bpm));
            Bpm = bpm;
            BeatLength = 60000d / Bpm;
        }

        /// <summary>
        /// 1~8拍子の中で最も近いものを取得する
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public int GetNearestSignature(double seconds)
        {
            if (Bpm <= 0) return 4;

            double beatSeconds = 60d / Bpm;
            double barSeconds = beatSeconds * 4d;

            int nearestSignature = 1;
            double minDiff = double.MaxValue;

            for (int i = 1; i <= 8; i++)
            {
                double targetSeconds = barSeconds / i;
                double diff = Math.Abs(seconds - targetSeconds);

                if (diff < minDiff)
                {
                    minDiff = diff;
                    nearestSignature = i;
                }
            }

            return nearestSignature;
        }

        public double GetExecuteTime(ExecuteRequestTiming timing, double accurateBeat)
        {
            if (Bpm <= 0) return 0;
            if (timing.Beat.Signature <= 0 || timing.Beat.Count <= 0 || timing.Beat.Count > timing.Beat.Signature)
            {
                throw new ArgumentOutOfRangeException(nameof(timing), "Beat must be within the bar.");
            }

            const double propTimeSignature = 4d;
            double currentBar = Math.Floor(accurateBeat / propTimeSignature);
            double targetBar = currentBar + timing.BarFlag;

            double barLengthMs = BeatLength * propTimeSignature;
            double targetBarStartTimingMs = targetBar * barLengthMs;
            double offsetInBarMs = (barLengthMs / timing.Beat.Signature) * (timing.Beat.Count - 1);
            return targetBarStartTimingMs + offsetInBarMs;
        }
    }
}