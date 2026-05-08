using System;

namespace KillChord.Runtime.Domain.InGame.Character
{
    /// <summary>
    ///     回避クールダウンを表すVO。
    /// </summary>
    public readonly struct DodgeCooldown
    {
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

        /// <summary>
        ///     回避クールダウンの値。
        /// </summary>
        public readonly float Value;

        public static explicit operator float(DodgeCooldown value)
            => value.Value;
        public static bool operator ==(DodgeCooldown left, DodgeCooldown right)
            => left.Value == right.Value;
        public static bool operator !=(DodgeCooldown left, DodgeCooldown right)
            => left.Value != right.Value;
        public override bool Equals(object obj)
        {
            return obj is DodgeCooldown dodgeCooldown && dodgeCooldown.Value == Value;
        }
        public bool Equals(DodgeCooldown other)
        {
            return other.Value == Value;
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
