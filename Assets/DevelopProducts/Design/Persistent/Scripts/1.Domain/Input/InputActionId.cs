using System;
using UnityEngine;

namespace DevelopProducts.Persistent.Domain.Input
{
    /// <summary>
    ///     入力アクションを識別するためのID。
    /// </summary>
    public readonly struct InputActionId : IEquatable<InputActionId>
    {
        public int Value { get; }

        public InputActionId(int value)
        {
            Value = value;
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public bool Equals(InputActionId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is InputActionId other && Equals(other);
        }

        public static bool operator ==(InputActionId left, InputActionId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InputActionId left, InputActionId right)
        {
            return !left.Equals(right);
        }
    }
}