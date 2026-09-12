using System;

namespace KillChord.Runtime.Domain.OutGame.Screen
{
    /// <summary>
    ///     制作メンバーの所属組織名を表す値オブジェクト。
    /// </summary>
    public readonly struct MemberAffiliationName : IEquatable<MemberAffiliationName>
    {
        /// <summary>
        ///     所属組織名を初期化します。
        /// </summary>
        /// <param name="value"> 所属組織名です。 </param>
        /// <exception cref="ArgumentException"> 所属組織名が null または空の場合に発生します。 </exception>
        public MemberAffiliationName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("所属組織名が null または空です。", nameof(value));
            }

            _value = value;
        }

        /// <summary> 所属組織名を取得します。 </summary>
        public string Value => _value;

        public static bool operator ==(MemberAffiliationName left, MemberAffiliationName right) => left.Equals(right);
        public static bool operator !=(MemberAffiliationName left, MemberAffiliationName right) => !left.Equals(right);

        /// <summary>
        ///     他の所属組織名と等価かどうかを判定します。
        /// </summary>
        /// <param name="other"> 比較対象です。 </param>
        /// <returns> 等価な場合はtrue。 </returns>
        public bool Equals(MemberAffiliationName other) => string.Equals(_value, other._value, StringComparison.Ordinal);

        /// <summary>
        ///     他のオブジェクトと等価かどうかを判定します。
        /// </summary>
        /// <param name="obj"> 比較対象です。 </param>
        /// <returns> 等価な場合はtrue。 </returns>
        public override bool Equals(object obj) => obj is MemberAffiliationName other && Equals(other);

        /// <summary>
        ///     ハッシュコードを取得します。
        /// </summary>
        /// <returns> ハッシュコードです。 </returns>
        public override int GetHashCode() => _value == null ? 0 : StringComparer.Ordinal.GetHashCode(_value);

        /// <summary>
        ///     文字列表現を取得します。
        /// </summary>
        /// <returns> 所属組織名です。 </returns>
        public override string ToString() => _value ?? string.Empty;

        private readonly string _value;
    }
}
