using KillChord.Runtime.Utility;
using System;

namespace KillChord.Runtime.Domain.InGame.Music
{
    /// <summary>
    ///     一拍の長さを定義するクラス。
    /// </summary>
    public readonly struct RhythmDefinition
    {
        /// <summary>
        ///     BPMを指定してリズム定義を生成する。
        /// </summary>
        /// <param name="bpm"> BPM。 </param>
        public RhythmDefinition(double bpm)
        {
            if (bpm <= 0) throw new ArgumentOutOfRangeException(nameof(bpm));
            _bpm = bpm;
            _beatLength = MusicConstants.SECONDS_PER_MINUTE / _bpm;
            _barLength = _beatLength * MusicConstants.STANDARD_BEATS_PER_BAR;
        }

        /// <summary> BPM。 </summary>
        public double Bpm => _bpm;
        /// <summary> 1拍の長さ（秒）。 </summary>
        public double BeatLength => _beatLength;
        /// <summary> 1小節の長さ（秒）。 </summary>
        public double BarLength => _barLength;

        /// <summary>
        ///     経過時間から経過小節数を計算する。
        /// </summary>
        /// <param name="durationSeconds"> 経過時間（秒）。 </param>
        /// <returns> 経過小節数。 </returns>
        public double CalculateElapsedBarCount(double durationSeconds)
        {
            if (Bpm <= 0) return 0d;

            return durationSeconds / _barLength;
        }

        /// <summary>
        ///     1〜8拍子の中で、指定された秒数に最も近い拍子を算出する。
        /// </summary>
        /// <param name="durationSeconds"> 前回のアクションからの経過秒数。 </param>
        /// <returns> 拍の種類。 </returns>
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