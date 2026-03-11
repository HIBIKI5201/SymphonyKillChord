using System;

namespace DevelopProducts.Persistent.Domain.Input
{
    public readonly struct InputMapId : IEquatable<InputMapId>
    {
        public int Value { get; }

        public InputMapId(int value)
        {
            Value = value;
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public bool Equals(InputMapId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is InputActionId other && Equals(other);
        }

        public static bool operator ==(InputMapId left, InputMapId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InputMapId left, InputMapId right)
        {
            return !left.Equals(right);
        }
    }
}