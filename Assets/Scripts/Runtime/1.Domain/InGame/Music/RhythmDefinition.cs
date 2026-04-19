using System;

namespace KillChord.Runtime.Domain.InGame.Music
{
    /// <summary>
    ///     一拍の長さを定義するクラス。
    /// </summary>
    public readonly struct RhythmDefinition
    {
        public RhythmDefinition(int bpm)
        {
            if (bpm <= 0) throw new ArgumentOutOfRangeException(nameof(bpm));
            Bpm = bpm;
            BeatLength = 60d / Bpm;
        }

        public readonly int Bpm;
        public readonly double BeatLength;

        /// <summary>
        /// 1~8拍子の中で、指定された秒数に最も近い拍子を算出する
        /// </summary>
        /// <param name="durationSeconds">前回のアクションからの経過秒数</param>
        /// <returns>1~8の拍子</returns>
        public BeatType CalculateBeatType(double durationSeconds)
        {
            if (Bpm <= 0)
            {
                return BeatType.Four;
            }

            double beatSeconds = 60d / Bpm;
            double barSeconds = beatSeconds * 4d;

            BeatType nearestBeatType = BeatType.Four;
            double minDiff = double.MaxValue;

            foreach (BeatType beatType in SupportedBeatTypes)
            {
                int signature = (int)beatType;
                double targetSeconds = barSeconds / signature;
                double diff = Math.Abs(durationSeconds - targetSeconds);

                if (diff < minDiff)
                {
                    minDiff = diff;
                    nearestBeatType = beatType;
                }
            }

            return nearestBeatType;
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

        private static readonly BeatType[] SupportedBeatTypes =
        {
            BeatType.One,
            BeatType.Two,
            BeatType.Three,
            BeatType.Four,
            BeatType.Six,
            BeatType.Eight
        };
    }
}