using System;

namespace KillChord.Runtime.Domain.InGame.Character
{
    /// <summary>
    ///     キャラクター定義を一意に識別するIDです。
    /// </summary>
    public readonly struct CharacterDefinitionId : IEquatable<CharacterDefinitionId>
    {
        /// <summary>
        ///     数値IDで初期化します。
        /// </summary>
        /// <param name="value"> 数値IDです。 </param>
        public CharacterDefinitionId(int value)
        {
            _value = value;
        }

        /// <summary> 数値IDです。 </summary>
        public int Value => _value;

        /// <summary>
        ///     別のIDと等価か判定します。
        /// </summary>
        /// <param name="other"> 比較対象です。 </param>
        /// <returns> 一致する場合はtrueです。 </returns>
        public bool Equals(CharacterDefinitionId other)
        {
            return _value == other._value;
        }

        /// <summary>
        ///     オブジェクトと等価か判定します。
        /// </summary>
        /// <param name="obj"> 比較対象です。 </param>
        /// <returns> 一致する場合はtrueです。 </returns>
        public override bool Equals(object obj)
        {
            return obj is CharacterDefinitionId other && Equals(other);
        }

        /// <summary>
        ///     ハッシュコードを取得します。
        /// </summary>
        /// <returns> 数値IDです。 </returns>
        public override int GetHashCode()
        {
            return _value;
        }

        /// <summary>
        ///     数値IDの文字列表現を取得します。
        /// </summary>
        /// <returns> 数値IDの文字列表現です。 </returns>
        public override string ToString()
        {
            return _value.ToString();
        }

        private readonly int _value;
    }
}
