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

        /// <summary>   BPM（テンポ）。</summary>
        public readonly int Bpm;
        /// <summary>   1拍の長さ（秒）。</summary>
        public readonly double BeatLength;

        /// <summary>
        /// 1~8拍子の中で、指定された秒数に最も近い拍子を算出する。
        /// </summary>
        /// <param name="durationSeconds">前回のアクションからの経過秒数</param>
        /// <returns>1~8の拍子</returns>
        public BeatType CalculateBeatType(double durationSeconds)
        {
            if (Bpm <= 0)
            {
                return BeatType.Four;
            }
            // 1拍の秒数と4拍子の秒数を計算。
            double beatSeconds = 60d / Bpm;
            // 4拍子の秒数は、1拍の秒数に4を掛けたもの。
            double barSeconds = beatSeconds * 4d;

            BeatType nearestBeatType = BeatType.Four;
            double minDiff = double.MaxValue;

            // 1~8拍子の中で、指定された秒数に最も近い拍子を算出する。
            foreach (BeatType beatType in SupportedBeatTypes)
            {
                // 各拍子の秒数を計算。例えば、2拍子なら、4拍子の秒数を2で割る。
                int signature = (int)beatType;
                // 例えば、2拍子なら、4拍子の秒数を2で割る。
                double targetSeconds = barSeconds / signature;
                // 経過秒数と目標秒数の差を計算。
                double diff = Math.Abs(durationSeconds - targetSeconds);

                if (diff < minDiff)
                {
                    // 最も近い拍子の差を更新。
                    minDiff = diff;
                    // 最も近い拍子を更新。
                    nearestBeatType = beatType;
                }
            }

            return nearestBeatType;
        }



        /// <summary>
        ///  ExecuteRequestTiming と現在の正確な拍位置から、実行時刻（秒）を算出する。
        /// </summary>
        /// <param name="timing"></param>
        /// <param name="accurateBeat"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
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