using System;

namespace KillChord.Runtime.Domain.InGame.Music
{
    /// <summary>
    ///     一拍の長さを定義するクラス。
    /// </summary>
    public readonly struct RhythmDefinition
    {
        public RhythmDefinition(double bpm)
        {
            if (bpm <= 0) throw new ArgumentOutOfRangeException(nameof(bpm));
            _bpm = bpm;
            _beatLength = SECONDS_PER_MINUTE / _bpm;
            _barLength = _beatLength * FOUR_FOUR_BEAT_COUNT;
        }

        public double Bpm => _bpm;
        public double BeatLength => _beatLength;
        public double BarLength => _barLength;

        public double CalculateElapsedBarCount(double durationSeconds)
        {
            if (Bpm <= 0) return 0d;

            return durationSeconds / _barLength;
        }

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

            BeatType nearestBeatType = BeatType.Four;
            double minDiff = double.MaxValue;

            foreach (BeatType beatType in SupportedBeatTypes)
            {
                int signature = (int)beatType;
                double targetSeconds = _barLength / signature;
                double diff = Math.Abs(durationSeconds - targetSeconds);

                if (diff < minDiff)
                {
                    minDiff = diff;
                    nearestBeatType = beatType;
                }
            }

            return nearestBeatType;
        }

        private const double SECONDS_PER_MINUTE = 60d;
        private const double FOUR_FOUR_BEAT_COUNT = 4d;

        private static readonly BeatType[] SupportedBeatTypes =
        {
            BeatType.One,
            BeatType.Two,
            BeatType.Three,
            BeatType.Four,
            BeatType.Six,
            BeatType.Eight
        };

        private readonly double _bpm;
        private readonly double _beatLength;
        private readonly double _barLength;
    }
}