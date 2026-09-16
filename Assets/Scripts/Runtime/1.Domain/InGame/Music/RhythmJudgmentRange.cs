using System;

namespace KillChord.Runtime.Domain.InGame.Music
{
    /// <summary>
    ///     リズム判定範囲を表す構造体。
    /// </summary>
    public readonly struct RhythmJudgmentRange : IEquatable<RhythmJudgmentRange>
    {
        /// <summary>
        ///     新しい判定範囲を生成する。
        /// </summary>
        /// <param name="beatType"> 拍の種類。 </param>
        /// <param name="startNormalized"> 開始位置（正規化）。 </param>
        /// <param name="endNormalized"> 終了位置（正規化）。 </param>
        /// <param name="justStartNormalized"> ジャスト開始位置（含む）。 </param>
        /// <param name="justEndNormalized"> ジャスト終了位置（含まない）。1小節を超える値も使用する。 </param>
        public RhythmJudgmentRange(BeatType beatType, float startNormalized, float endNormalized,
            float justStartNormalized, float justEndNormalized)
        {
            if (startNormalized < 0f || startNormalized > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(startNormalized));
            }

            if (endNormalized < startNormalized || endNormalized > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(endNormalized));
            }

            if (float.IsNaN(justStartNormalized) || float.IsInfinity(justStartNormalized) || justStartNormalized < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(justStartNormalized));
            }

            if (float.IsNaN(justEndNormalized) || float.IsInfinity(justEndNormalized) || justEndNormalized <= justStartNormalized)
            {
                throw new ArgumentOutOfRangeException(nameof(justEndNormalized));
            }

            _beatType = beatType;
            _startNormalized = startNormalized;
            _endNormalized = endNormalized;
            _justStartNormalized = justStartNormalized;
            _justEndNormalized = justEndNormalized;
        }

        /// <summary> 拍の種類。 </summary>
        public BeatType BeatType => _beatType;
        /// <summary> 開始位置（正規化）。 </summary>
        public float StartNormalized => _startNormalized;
        /// <summary> 終了位置（正規化）。 </summary>
        public float EndNormalized => _endNormalized;
        /// <summary> ジャスト開始位置（含む）。 </summary>
        public float JustStartNormalized => _justStartNormalized;
        /// <summary> ジャスト終了位置（含まない）。 </summary>
        public float JustEndNormalized => _justEndNormalized;

        /// <summary>
        ///     クランプ前の小節進捗がジャスト範囲に含まれるか判定する。
        /// </summary>
        /// <param name="barProgress"> 直前の入力からの小節進捗。 </param>
        /// <returns> ジャスト範囲内の場合はtrue。 </returns>
        public bool ContainsJustTiming(float barProgress)
        {
            return barProgress >= _justStartNormalized && barProgress < _justEndNormalized;
        }

        /// <summary>
        ///     指定された時間が範囲内に含まれるか判定する。
        /// </summary>
        /// <param name="normalizedBarProgress"> 小節内の正規化進捗。 </param>
        /// <returns> 範囲内であれば true。 </returns>
        public bool Contains(float normalizedBarProgress)
        {
            return normalizedBarProgress >= _startNormalized && normalizedBarProgress <= _endNormalized;
        }

        /// <summary>
        ///     等価比較を行う。
        /// </summary>
        /// <param name="other"> 比較対象。 </param>
        /// <returns> 等しければ true。 </returns>
        public bool Equals(RhythmJudgmentRange other)
        {
            return _beatType == other._beatType &&
                     _startNormalized.Equals(other._startNormalized) &&
                     _endNormalized.Equals(other._endNormalized) &&
                     _justStartNormalized.Equals(other._justStartNormalized) &&
                     _justEndNormalized.Equals(other._justEndNormalized);
        }

        private readonly BeatType _beatType;
        private readonly float _startNormalized;
        private readonly float _endNormalized;
        private readonly float _justStartNormalized;
        private readonly float _justEndNormalized;
    }
}
