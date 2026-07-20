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
        /// <param name="beatOffsetSeconds"> 音源先頭から最初の小節頭までの秒数。 </param>
        public RhythmDefinition(double bpm, double beatOffsetSeconds = 0d)
        {
            if (bpm <= 0d || double.IsNaN(bpm) || double.IsInfinity(bpm))
            {
                throw new ArgumentOutOfRangeException(nameof(bpm));
            }

            if (double.IsNaN(beatOffsetSeconds) || double.IsInfinity(beatOffsetSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(beatOffsetSeconds));
            }

            _bpm = bpm;
            _beatLength = MusicConstants.SECONDS_PER_MINUTE / _bpm;
            _barLength = _beatLength * MusicConstants.STANDARD_BEATS_PER_BAR;
            _beatOffsetSeconds = beatOffsetSeconds;
        }

        /// <summary> BPM。 </summary>
        public double Bpm => _bpm;
        /// <summary> 1拍の長さ（秒）。 </summary>
        public double BeatLength => _beatLength;
        /// <summary> 1小節の長さ（秒）。 </summary>
        public double BarLength => _barLength;
        /// <summary> 音源先頭から最初の小節頭までの秒数。 </summary>
        public double BeatOffsetSeconds => _beatOffsetSeconds;

        /// <summary>
        ///     音源の再生時間から、オフセットを考慮した正確な拍位置を計算する。
        /// </summary>
        /// <param name="playTimeSeconds"> 音源の再生時間（秒）。 </param>
        /// <returns> 最初の小節頭を0とする拍位置。 </returns>
        public double CalculateAccurateBeat(double playTimeSeconds)
        {
            if (Bpm <= 0d || double.IsNaN(playTimeSeconds) || double.IsInfinity(playTimeSeconds))
            {
                return 0d;
            }

            double elapsedSeconds = playTimeSeconds - BeatOffsetSeconds;
            return elapsedSeconds > 0d ? elapsedSeconds / BeatLength : 0d;
        }

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
        private readonly double _beatOffsetSeconds;
    }
}
