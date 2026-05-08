using System;

namespace KillChord.Runtime.Domain.InGame.Character
{
    /// <summary>
    ///     回避の持続時間を表すVO。
    /// </summary>
    public readonly struct DodgeDuration
    {
        public DodgeDuration(float value)
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
        ///    回避の持続時間の値。  
        /// </summary>
        public readonly float Value;

        public static explicit operator float(DodgeDuration value)
            => value.Value;
        public static bool operator ==(DodgeDuration left, DodgeDuration right)
            => left.Value == right.Value;
        public static bool operator !=(DodgeDuration left, DodgeDuration right)
            => left.Value != right.Value;
        public override bool Equals(object obj)
        {
            return obj is DodgeDuration dodgeDuration && dodgeDuration.Value == Value;
        }
        public bool Equals(DodgeDuration other)
        {
            return other.Value == Value;
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
