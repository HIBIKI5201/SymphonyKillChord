using System;

namespace KillChord.Runtime.Domain.OutGame.Scenario
{
    /// <summary>
    ///     シナリオアニメーションを一意に識別するIDです。
    /// </summary>
    public readonly struct AnimationId : IEquatable<AnimationId>
    {
        /// <summary>
        ///     数値IDで初期化します。
        /// </summary>
        /// <param name="value"> 数値IDです。 </param>
        public AnimationId(int value)
        {
            if (value == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "AnimationIdに0は使用できません。");
            }

            Value = value;
        }

        /// <summary> 数値IDです。 </summary>
        public int Value { get; }

        /// <summary>
        ///     別のIDと等価か判定します。
        /// </summary>
        /// <param name="other"> 比較対象です。 </param>
        /// <returns> 一致する場合はtrueです。 </returns>
        public bool Equals(AnimationId other) => Value == other.Value;

        /// <summary>
        ///     オブジェクトと等価か判定します。
        /// </summary>
        /// <param name="obj"> 比較対象です。 </param>
        /// <returns> 一致する場合はtrueです。 </returns>
        public override bool Equals(object obj) => obj is AnimationId other && Equals(other);

        /// <summary>
        ///     ハッシュコードを取得します。
        /// </summary>
        /// <returns> 数値IDです。 </returns>
        public override int GetHashCode() => Value;

        /// <summary>
        ///     数値IDの文字列表現を取得します。
        /// </summary>
        /// <returns> 数値IDの文字列表現です。 </returns>
        public override string ToString() => Value.ToString();
    }
}
