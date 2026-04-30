using System;

namespace KillChord.Runtime.Domain.InGame.Parameter
{
    /// <summary>
    ///     回避速度を表すVO。
    /// </summary>
    public readonly struct DodgeSpeed
    {
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

        /// <summary>
        ///      回避速度の値。
        /// </summary>
        public readonly float Value;

        public static explicit operator float(DodgeSpeed value)
            => value.Value;
        public static bool operator ==(DodgeSpeed left, DodgeSpeed right)
            => left.Value == right.Value;
        public static bool operator !=(DodgeSpeed left, DodgeSpeed right)
            => left.Value != right.Value;
        public override bool Equals(object obj)
        {
            return obj is DodgeSpeed dodgeSpeed && dodgeSpeed.Value == Value;
        }
        public bool Equals(DodgeSpeed other)
        {
            return other.Value == Value;
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
