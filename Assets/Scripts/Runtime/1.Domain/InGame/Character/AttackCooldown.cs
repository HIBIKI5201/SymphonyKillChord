using System;

namespace KillChord.Runtime.Domain.InGame.Character
{
    /// <summary>
    ///     攻撃クールダウンを表すVO。
    /// </summary>
    public readonly struct AttackCooldown
    {
        /// <summary>
        ///     攻撃クールダウンを生成する。
        ///     負の値や有限でない値は例外を投げる。
        /// </summary>
        public AttackCooldown(float value)
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

        /// <summary> 攻撃クールダウンの値。 </summary>
        public readonly float Value;

        /// <summary>
        ///     float へ明示的に変換する。
        /// </summary>
        public static explicit operator float(AttackCooldown value)
            => value.Value;

        public static bool operator ==(AttackCooldown left, AttackCooldown right)
            => left.Value == right.Value;

        public static bool operator !=(AttackCooldown left, AttackCooldown right)
            => left.Value != right.Value;

        /// <summary>
        ///     他のオブジェクトと値が等しいかを判定する。
        /// </summary>
        public override bool Equals(object obj)
            => obj is AttackCooldown cooldown && cooldown.Value == Value;

        /// <summary>
        ///     他の攻撃クールダウンと値が等しいかを判定する。
        /// </summary>
        public bool Equals(AttackCooldown other)
            => other.Value == Value;

        /// <summary>
        ///     値から算出したハッシュコードを返す。
        /// </summary>
        public override int GetHashCode()
            => Value.GetHashCode();
    }
}