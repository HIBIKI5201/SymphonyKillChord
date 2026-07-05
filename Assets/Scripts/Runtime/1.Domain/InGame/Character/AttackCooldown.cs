using System;

namespace KillChord.Runtime.Domain.InGame.Character
{
    /// <summary>
    ///     攻撃クールダウンを表すVO。
    /// </summary>
    public readonly struct AttackCooldown
    {
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

        public static explicit operator float(AttackCooldown value)
            => value.Value;

        public static bool operator ==(AttackCooldown left, AttackCooldown right)
            => left.Value == right.Value;

        public static bool operator !=(AttackCooldown left, AttackCooldown right)
            => left.Value != right.Value;

        public override bool Equals(object obj)
            => obj is AttackCooldown cooldown && cooldown.Value == Value;

        public bool Equals(AttackCooldown other)
            => other.Value == Value;

        public override int GetHashCode()
            => Value.GetHashCode();
    }
}