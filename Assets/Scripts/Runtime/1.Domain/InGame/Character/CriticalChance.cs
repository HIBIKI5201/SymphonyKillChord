using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Character
{
    /// <summary>
    ///     クリティカル率を表す値オブジェクト。
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
            _value = Mathf.Clamp01(value);
        }

        /// <summary>
        ///     クリティカル率を0から1の範囲で表す値。
        /// </summary>
        public float Value => _value;

        /// <summary>
        ///     指定された値より小さいかどうかを判定します。
        /// </summary>
        public static bool operator <(CriticalChance left, float right)
        {
            // 発生率の値が右辺より小さいか判定します。
            return left._value < right;
        }

        /// <summary>
        ///     指定された値より大きいかどうかを判定します。
        /// </summary>
        public static bool operator >(CriticalChance left, float right)
        {
            // 発生率の値が右辺より大きいか判定します。
            return left._value > right;
        }

        private readonly float _value;
    }
}
