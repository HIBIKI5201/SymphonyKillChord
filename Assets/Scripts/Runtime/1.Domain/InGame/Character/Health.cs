using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Character
{
    /// <summary>
    ///     体力値を表す値オブジェクト。
    /// </summary>
    public readonly struct Health : IEquatable<Health>
    {
        public Health(float value)
        {
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