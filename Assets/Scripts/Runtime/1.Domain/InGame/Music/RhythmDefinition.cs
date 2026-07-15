using KillChord.Runtime.Utility.Constant;
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
        ///     経過時間を小節長で正規化した進捗を計算する。
        /// </summary>
        /// <param name="durationSeconds"> 前回のアクションからの経過秒数。 </param>
        /// <returns> 0〜1に正規化された小節進捗。 </returns>
        public float CalculateNormalizedBarProgress(double durationSeconds)
        {
            if (Bpm <= 0)
            {
                return 0f;
            }

            double elapsedBarCount = CalculateElapsedBarCount(durationSeconds);
            if (elapsedBarCount <= 0d)
            {
                return 0f;
            }

            if (elapsedBarCount >= 1d)
            {
                return 1f;
            }

            return (float)elapsedBarCount;
        }

        private readonly double _bpm;
        private readonly double _beatLength;
        private readonly double _barLength;
    }
}
