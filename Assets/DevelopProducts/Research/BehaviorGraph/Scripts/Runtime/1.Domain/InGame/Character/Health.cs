using System;
using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Character
{
    /// <summary>
    ///     体力値を表す値オブジェクト。
    /// </summary>
    public readonly struct Health : IEquatable<Health>
    {
        public Health(float value)
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

        public static explicit operator float(Health health) => health.Value;

        public override bool Equals(object obj)
        {
            return obj is Health health && Value.Equals(health.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public bool Equals(Health other)
        {
            return Value.Equals(other.Value);
        }

        public readonly float Value;
    }
}