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
        public MissionId(int value)
        {
            if (value == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "MissionIdに0は使用できません。");
            }

            _value = value;
        }

        /// <summary> IDの値を取得します。 </summary>
        public int Value => _value;

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
        public override int GetHashCode() => _value;
        /// <summary>
        ///     文字列形式を取得します。
        /// </summary>
        /// <returns>文字列形式。</returns>
        public override string ToString() => _value.ToString();

        /// <summary> IDの値。 </summary>
        private readonly int _value;
    }
}
