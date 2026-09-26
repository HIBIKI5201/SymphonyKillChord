using System;

namespace KillChord.Runtime.Domain.InGame.StatusEffect
{
    /// <summary>
    ///     状態効果を識別する値オブジェクト。
    /// </summary>
    public readonly struct StatusEffectId : IEquatable<StatusEffectId>
    {
        /// <summary>
        ///     状態効果 ID を生成する。
        ///     空文字の場合は例外を投げる。
        /// </summary>
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

        /// <summary>
        ///     他の状態効果 ID と文字列が一致するかを判定する。
        /// </summary>
        public bool Equals(StatusEffectId other)
        {
            return _value == other._value;
        }

        /// <summary>
        ///     他のオブジェクトと値が等しいかを判定する。
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is StatusEffectId other && Equals(other);
        }

        /// <summary>
        ///     文字列値から算出したハッシュコードを返す。
        /// </summary>
        public override int GetHashCode()
        {
            return _value != null ? StringComparer.Ordinal.GetHashCode(_value) : 0;
        }

        /// <summary>
        ///     状態効果 ID の文字列を返す。
        /// </summary>
        public override string ToString()
        {
            return _value;
        }

        private readonly string _value;
    }
}
