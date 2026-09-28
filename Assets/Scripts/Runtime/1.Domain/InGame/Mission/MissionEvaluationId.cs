using System;

namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     サブミッションの評価IDを表す値オブジェクト。
    /// </summary>
    public readonly struct MissionEvaluationId : IEquatable<MissionEvaluationId>
    {
        /// <summary>
        ///    サブミッションの評価IDを表す値オブジェクトを初期化します。
        /// </summary>
        /// <param name="value"> サブミッションの評価IDの文字列値。 </param>
        /// <exception cref="ArgumentException"> valueがnull、空、または空白の場合にスローされます。 </exception>
        public MissionEvaluationId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("ミッションの評価IDが空です。", nameof(value));
            }

            _value = value;
        }

        /// <summary> 評価 ID の文字列値。 </summary>
        public string Value => _value;

        /// <summary>
        ///     他の評価 ID と文字列が一致するかを判定する。
        /// </summary>
        public bool Equals(MissionEvaluationId other)
        {
            return string.Equals(
                _value,
                other._value,
                StringComparison.Ordinal);
        }

        /// <summary>
        ///     他のオブジェクトと値が等しいかを判定する。
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is MissionEvaluationId other
                && Equals(other);
        }

        /// <summary>
        ///     文字列値から算出したハッシュコードを返す。
        /// </summary>
        public override int GetHashCode()
        {
            return _value == null ? 0 : StringComparer.Ordinal.GetHashCode(_value);
        }

        /// <summary>
        ///     評価 ID の文字列を返す。未設定の場合は空文字を返す。
        /// </summary>
        public override string ToString()
        {
            return _value ?? string.Empty;
        }

        public static bool operator ==(MissionEvaluationId left, MissionEvaluationId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MissionEvaluationId left, MissionEvaluationId right)
        {
            return !left.Equals(right);
        }

        private readonly string _value;
    }
}
