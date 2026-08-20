using System;

namespace KillChord.Runtime.Domain.OutGame.Screen
{
    /// <summary>
    ///     制作メンバーの役職名を表す値オブジェクト。
    /// </summary>
    public readonly struct MemberClassName : IEquatable<MemberClassName>
    {
        /// <summary>
        ///     役職名を初期化します。
        /// </summary>
        /// <param name="value"> 役職名です。 </param>
        /// <exception cref="ArgumentException"> 役職名が null または空の場合に発生します。 </exception>
        public MemberClassName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("役職名が null または空です。", nameof(value));
            }

            _value = value;
        }

        /// <summary> 役職名を取得します。 </summary>
        public string Value => _value;

        public static bool operator ==(MemberClassName left, MemberClassName right) => left.Equals(right);
        public static bool operator !=(MemberClassName left, MemberClassName right) => !left.Equals(right);

        /// <summary>
        ///     他の役職名と等価かどうかを判定します。
        /// </summary>
        /// <param name="other"> 比較対象です。 </param>
        /// <returns> 等価な場合はtrue。 </returns>
        public bool Equals(MemberClassName other) => string.Equals(_value, other._value, StringComparison.Ordinal);

        /// <summary>
        ///     他のオブジェクトと等価かどうかを判定します。
        /// </summary>
        /// <param name="obj"> 比較対象です。 </param>
        /// <returns> 等価な場合はtrue。 </returns>
        public override bool Equals(object obj) => obj is MemberClassName other && Equals(other);

        /// <summary>
        ///     ハッシュコードを取得します。
        /// </summary>
        /// <returns> ハッシュコードです。 </returns>
        public override int GetHashCode() => _value == null ? 0 : StringComparer.Ordinal.GetHashCode(_value);

        /// <summary>
        ///     文字列表現を取得します。
        /// </summary>
        /// <returns> 役職名です。 </returns>
        public override string ToString() => _value ?? string.Empty;

        private readonly string _value;
    }
}
