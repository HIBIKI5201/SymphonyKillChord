using System;

namespace KillChord.Runtime.Domain.InGame.Character
{
    /// <summary>
    ///     最小射程距離を表す値オブジェクト。
    /// </summary>
    public readonly struct AttackRangeMin
    {
        public AttackRangeMin(float value)
        {
            if (value < 0)
            {
                throw new ArgumentException("value must be non-negative.", nameof(value));
            }
            if (!float.IsFinite(value))
            {
                throw new ArgumentException("Damage must be finite.", nameof(value));
            }

            _value = value < 0f ? 0f : value;
        }

        public float Value => _value;

        public bool Equals(AttackRangeMin other)
        {
            return _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            return obj is AttackRangeMin other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        private readonly float _value;
    }
}
