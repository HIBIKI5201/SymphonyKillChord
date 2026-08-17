using System;

namespace KillChord.Runtime.Domain.InGame.StatusEffect
{
    /// <summary>
    ///     状態効果を識別する値オブジェクト。
    /// </summary>
    public readonly struct StatusEffectId : IEquatable<StatusEffectId>
    {
        public StatusEffectId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("状態効果IDを指定してください。", nameof(value));
            }

            _value = value;
        }

        /// <summary> 状態効果ID </summary>
        public string Value => _value;

        public static bool operator ==(StatusEffectId left, StatusEffectId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StatusEffectId left, StatusEffectId right)
        {
            return !left.Equals(right);
        }

        public bool Equals(StatusEffectId other)
        {
            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            return obj is StatusEffectId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _value != null ? StringComparer.Ordinal.GetHashCode(_value) : 0;
        }

        public override string ToString()
        {
            return _value;
        }

        private readonly string _value;
    }
}
