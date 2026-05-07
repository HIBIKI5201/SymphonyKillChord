using System;

namespace KillChord.Runtime.Domain.InGame.Music
{
    /// <summary>
    ///     リズムガイドの判定範囲を表す構造体。
    /// </summary>
    public readonly struct RhythmGuideRange : IEquatable<RhythmGuideRange>
    {
        /// <summary>
        ///     新しい判定範囲を生成する。
        /// </summary>
        /// <param name="beatType"> 拍の種類。 </param>
        /// <param name="startNormalized"> 開始位置（正規化）。 </param>
        /// <param name="endNormalized"> 終了位置（正規化）。 </param>
        public RhythmGuideRange(BeatType beatType, float startNormalized, float endNormalized)
        {
            if (startNormalized < 0f || startNormalized > 1f) throw new ArgumentOutOfRangeException(nameof(startNormalized));
            if (endNormalized < startNormalized || endNormalized > 1f) throw new ArgumentOutOfRangeException(nameof(endNormalized));

            _beatType = beatType;
            _startNormalized = startNormalized;
            _endNormalized = endNormalized;
        }

        /// <summary> 拍の種類。 </summary>
        public BeatType BeatType => _beatType;
        /// <summary> 開始位置（正規化）。 </summary>
        public float StartNormalized => _startNormalized;
        /// <summary> 終了位置（正規化）。 </summary>
        public float EndNormalized => _endNormalized;

        /// <summary>
        ///     指定された時間が範囲内に含まれるか判定する。
        /// </summary>
        /// <param name="elapsedBeat"> 小節内の経過拍数。 </param>
        /// <returns> 範囲内であれば true。 </returns>
        public bool Contains(float elapsedBeat)
        {
            return elapsedBeat >= _startNormalized && elapsedBeat <= _endNormalized;
        }

        /// <summary>
        ///     等価比較を行う。
        /// </summary>
        /// <param name="other"> 比較対象。 </param>
        /// <returns> 等しければ true。 </returns>
        public bool Equals(RhythmGuideRange other)
        {
            return _beatType == other._beatType &&
                     _startNormalized.Equals(other._startNormalized) &&
                     _endNormalized.Equals(other._endNormalized);
        }

        private readonly BeatType _beatType;
        private readonly float _startNormalized;
        private readonly float _endNormalized;
    }
}
