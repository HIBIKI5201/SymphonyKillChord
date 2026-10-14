# 設計思想の実装例

規則は [DesignClassRoles.md](DesignClassRoles.md) を参照。ここは実装時に必要な例だけを読む。

## Entity

- サンプルコード

    ```csharp
    public class Entity
    {
        public Entity(string id)
        {
            _id = id;
            _value = 0;
        }

        public string ID => _id;
        public float Value => _value;

        public void ChangeValue(float value) => _value = value;

        private readonly string _id;
        private float _value;
    }
    ```

## ValueObject

- サンプルコード

    ```csharp
    public readonly struct ValueObject : IEquatable<ValueObject>, IComparable<ValueObject>
    {
        public ValueObject(float value = 0)
        {
            if (value < 0) { throw new ArgumentException("value is can't negative", nameof(value)); }
            _value = value;
        }

        public float Value => _value;

        public static bool operator ==(ValueObject left, ValueObject right) => left.Equals(right);
        public static bool operator !=(ValueObject left, ValueObject right) => !left.Equals(right);
        public static bool operator <(ValueObject left, ValueObject right) => left.CompareTo(right) < 0;
        public static bool operator <=(ValueObject left, ValueObject right) => left.CompareTo(right) <= 0;
        public static bool operator >(ValueObject left, ValueObject right) => left.CompareTo(right) > 0;
        public static bool operator >=(ValueObject left, ValueObject right) => left.CompareTo(right) >= 0;

        public int CompareTo(ValueObject other) => _value.CompareTo(other._value);
        public bool Equals(ValueObject other) => _value == other._value;
        public override bool Equals(object obj) => obj is ValueObject other && Equals(other);
        public override int GetHashCode() => _value.GetHashCode();

        private readonly float _value;
    }
    ```
