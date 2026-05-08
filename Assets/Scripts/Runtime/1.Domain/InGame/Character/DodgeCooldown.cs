using System;

namespace KillChord.Runtime.Domain.InGame.Character
{
    /// <summary>
    ///     回避クールダウンを表すVO。
    /// </summary>
    public readonly struct DodgeCooldown
    {
        /// <summary>
        ///     回避クールダウンを初期化するコンストラクタ。
        /// </summary>
        /// <param name="value"></param>
        public DodgeCooldown(float value)
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

        /// <summary> 回避クールダウンの値。 </summary>
        public readonly float Value;

        /// <summary>
        ///     floatへの明示的型変換を行う。
        /// </summary>
        public static explicit operator float(DodgeCooldown value)
            => value.Value;

        /// <summary>
        ///     等価比較を行う。
        /// </summary>
        public static bool operator ==(DodgeCooldown left, DodgeCooldown right)
            => left.Value == right.Value;

        /// <summary>
        ///     不等価比較を行う。
        /// </summary>
        public static bool operator !=(DodgeCooldown left, DodgeCooldown right)
            => left.Value != right.Value;

        /// <summary>
        ///     等価比較を行う。
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is DodgeCooldown dodgeCooldown && dodgeCooldown.Value == Value;
        }

        /// <summary>
        ///     等価比較を行う。
        /// </summary>
        public bool Equals(DodgeCooldown other)
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
