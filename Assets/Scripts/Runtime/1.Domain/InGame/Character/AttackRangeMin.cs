using System;

namespace KillChord.Runtime.Domain.InGame.Character
{
    /// <summary>
    ///     最小射程距離を表す値オブジェクト。
    /// </summary>
    public readonly struct AttackRangeMin
    {
        /// <summary>
        ///     最小射程距離を初期化するコンストラクタ。
        /// </summary>
        /// <param name="value"></param>
        public AttackRangeMin(float value)
        {
            if (value < 0)
            {
                throw new ArgumentException("value must be non-negative.", nameof(value));
            }
            if (!float.IsFinite(value))
            {
                throw new ArgumentException("value must be finite.", nameof(value));
            }

            _value = value < 0f ? 0f : value;
        }

        /// <summary> 最小射程距離を取得する。 </summary>
        public float Value => _value;

        /// <summary>
        ///     等価比較を行う。
        /// </summary>
        public bool Equals(AttackRangeMin other)
        {
            return _value.Equals(other._value);
        }

        /// <summary>
        ///     等価比較を行う。
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is AttackRangeMin other && Equals(other);
        }

        /// <summary>
        ///     ハッシュ値を計算する。
        /// </summary>
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        private readonly float _value;
    }
}
