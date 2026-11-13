using System;

namespace KillChord.Runtime.Domain.InGame.Character
{
    /// <summary>
    ///     クリティカル率を表す値オブジェクト。
    ///     1(100%)を超える値は、1ごとにクリティカルが確定で1回重なることを表す。
    /// </summary>
    public readonly struct CriticalChance
    {
        /// <summary>
        ///     クリティカル率のインスタンスを初期化するコンストラクタ。
        /// </summary>
        /// <param name="value"></param>
        public CriticalChance(float value)
        {
            if (!float.IsFinite(value))
            {
                throw new ArgumentException("value must be finite.", nameof(value));
            }

            if (value < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "CriticalChanceは0以上である必要があります。");
            }

            _value = value;
        }

        /// <summary>
        ///     クリティカル率を取得する。1が100%で、1を超える場合はクリティカルの重複を含む。
        /// </summary>
        public float Value => _value;

        /// <summary>
        ///     このクリティカル率で発生するクリティカルの回数を抽選する。
        ///     整数部の回数は確定で、小数部は1回追加される確率になる。
        /// </summary>
        /// <param name="randomValue"> 0以上1未満の乱数。 </param>
        /// <returns> クリティカルの回数。0の場合はクリティカルなし。 </returns>
        public int RollCriticalCount(float randomValue)
        {
            int guaranteedCount = (int)MathF.Floor(_value);
            float extraChance = _value - guaranteedCount;
            return randomValue < extraChance ? guaranteedCount + 1 : guaranteedCount;
        }

        /// <summary>
        ///     指定された値より小さいかどうかを判定する。
        /// </summary>
        public static bool operator <(CriticalChance left, float right)
        {
            // 発生率の値が右辺より小さいか判定する。
            return left._value < right;
        }

        /// <summary>
        ///     指定された値より大きいかどうかを判定する。
        /// </summary>
        public static bool operator >(CriticalChance left, float right)
        {
            // 発生率の値が右辺より大きいか判定する。
            return left._value > right;
        }

        private readonly float _value;
    }
}
