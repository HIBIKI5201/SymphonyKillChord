using System;

namespace KillChord.Runtime.Domain.OutGame.StageSelect
{
    /// <summary>
    ///     ステージの ID を表す値型オブジェクト。
    /// </summary>
    public readonly struct StageId : IEquatable<StageId>
    {
        /// <summary>
        ///     ステージ ID を初期化する。
        /// </summary>
        /// <param name="value"> ステージ ID の値。 </param>
        /// <exception cref="ArgumentException"> ステージ ID が null または空文字の場合にスローされる。 </exception>
        public StageId(string value)
        {
            // ステージ ID は null または空文字であってはならない。
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("StageId cannot be null or empty", nameof(value));
            }

            _value = value;
        }

        public string Value => _value;

        /// <summary>
        ///     ステージ ID の等値比較を行う。
        /// </summary>
        /// <param name="other"> 比較対象のステージ ID。 </param>
        /// <returns> 等しい場合は true、それ以外の場合は false。 </returns>
        public bool Equals(StageId other)
        {
            return _value == other._value;
        }

        /// <summary>
        ///     ステージ ID の等値比較を行う。
        /// </summary>
        /// <param name="obj"> 比較対象のオブジェクト。 </param>
        /// <returns> 等しい場合は true、それ以外の場合は false。 </returns>
        public override bool Equals(object obj)
        {
            return obj is StageId other && Equals(other);
        }

        /// <summary>
        ///     ステージ ID のハッシュコードを取得する。
        /// </summary>
        /// <returns> ステージ ID のハッシュコード。 </returns>
        public override int GetHashCode()
        {
            return _value != null ? _value.GetHashCode() : 0;
        }

        /// <summary>
        ///     ステージ ID の文字列表現を取得する。
        /// </summary>
        /// <returns> ステージ ID の文字列表現。 </returns>
        public override string ToString()
        {
            return _value;
        }

        private readonly string _value;
    }
}