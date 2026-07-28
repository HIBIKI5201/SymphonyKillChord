using System;

namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     ミッションを識別するためのIDを表す値オブジェクト。
    /// </summary>
    public readonly struct MissionId : IEquatable<MissionId>
    {
        /// <summary>
        ///     MissionId 構造体の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="value">IDの値。</param>
        public MissionId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("MissionIdが空文字列またはnullです。", nameof(value));
            }

            _value = value;
        }

        /// <summary> IDの値を取得します。 </summary>
        public string Value => _value;

        /// <summary>
        ///     他のオブジェクトと等しいかどうかを判定します。
        /// </summary>
        /// <param name="other">比較対象のオブジェクト。</param>
        /// <returns>等しい場合は true、そうでない場合は false。</returns>
        public bool Equals(MissionId other) => _value == other._value;
        /// <summary>
        ///     他のオブジェクトと等しいかどうかを判定します。
        /// </summary>
        /// <param name="obj">比較対象のオブジェクト。</param>
        /// <returns>等しい場合は true、そうでない場合は false。</returns>
        public override bool Equals(object obj) => obj is MissionId other && Equals(other);
        /// <summary>
        ///     ハッシュコードを取得します。
        /// </summary>
        /// <returns>ハッシュコード。</returns>
        public override int GetHashCode() => _value != null ? _value.GetHashCode() : 0;
        /// <summary>
        ///     文字列形式を取得します。
        /// </summary>
        /// <returns>文字列形式。</returns>
        public override string ToString() => _value;

        /// <summary> IDの値。 </summary>
        private readonly string _value;
    }
}
