using System;

namespace KillChord.Runtime.Domain.InGame.Character
{
    /// <summary>
    ///     回避速度を表すVO。
    /// </summary>
    public readonly struct DodgeSpeed
    {
        /// <summary>
        ///     回避速度を初期化するコンストラクタ。
        /// </summary>
        /// <param name="value"></param>
        public DodgeSpeed(float value)
        {
            if (value < 0)
            {
                throw new ArgumentException("value must be non-negative.", nameof(value));
            }
            if (!float.IsFinite(value))
            {
                throw new ArgumentException("value must be finite.", nameof(value));
            }

            Value = value;
        }

        /// <summary> 回避速度の値。 </summary>
        public readonly float Value;

        /// <summary>
        ///     floatへの明示的型変換を行う。
        /// </summary>
        public static explicit operator float(DodgeSpeed value)
            => value.Value;

        /// <summary>
        ///     等価比較を行う。
        /// </summary>
        public static bool operator ==(DodgeSpeed left, DodgeSpeed right)
            => left.Value == right.Value;

        /// <summary>
        ///     不等価比較を行う。
        /// </summary>
        public static bool operator !=(DodgeSpeed left, DodgeSpeed right)
            => left.Value != right.Value;

        /// <summary>
        ///     等価比較を行う。
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is DodgeSpeed dodgeSpeed && dodgeSpeed.Value == Value;
        }

        /// <summary>
        ///     等価比較を行う。
        /// </summary>
        public bool Equals(DodgeSpeed other)
        {
            return other.Value == Value;
        }

        /// <summary>
        ///     ハッシュ値を計算する。
        /// </summary>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
