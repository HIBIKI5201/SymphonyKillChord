using System;

namespace DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Character
{
    /// <summary>
    ///     移動速度を表す値オブジェクト。
    /// </summary>
    public readonly struct MoveSpeed
    {
        public MoveSpeed(float value)
        {
            if (value < 0)
            {
                throw new ArgumentException("value must be non-negative.", nameof(value));
            }
            if (!float.IsFinite(value))
            {
                throw new ArgumentException("Damage must be finite.", nameof(value));
            }

            Value = value;
        }

        public readonly float Value;

        public static explicit operator float(MoveSpeed value)
            => value.Value;
        public static bool operator ==(MoveSpeed left, MoveSpeed right)
            => left.Value == right.Value;
        public static bool operator !=(MoveSpeed left, MoveSpeed right)
            => left.Value != right.Value;
        public override bool Equals(object obj)
        {
            return obj is MoveSpeed moveSpeed && moveSpeed.Value == Value;
        }
        public bool Equals(MoveSpeed other)
        {
            return other.Value == Value;
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
