using System;
using UnityEngine;

namespace DevelopProducts.Persistent.Domain.Input
{
    /// <summary>
    ///     入力フェーズの識別子を表すクラス。
    /// </summary>
    public class InputPheseId : IEquatable<InputPheseId>
    {
        public int Value { get; }

        public InputPheseId(int value)
        {
            Value = value;
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public bool Equals(InputPheseId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is InputPheseId other && Equals(other);
        }

        public static bool operator ==(InputPheseId left, InputPheseId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InputPheseId left, InputPheseId right)
        {
            return !left.Equals(right);
        }
    }
}