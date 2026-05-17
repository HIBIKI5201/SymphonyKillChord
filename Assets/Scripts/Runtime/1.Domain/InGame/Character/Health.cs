using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Character
{
    /// <summary>
    ///     体力値を表す値オブジェクト。
    /// </summary>
    public readonly struct Health : IEquatable<Health>
    {
        /// <summary>
        ///     体力値を初期化するコンストラクタ。
        /// </summary>
        /// <param name="value"></param>
        public Health(float value)
        {
            if (value < 0)
            {
                throw new ArgumentException("value must be non-negative.", nameof(value));
            }
            if (!float.IsFinite(value))
            {
                throw new ArgumentException("Damage must be finite.", nameof(value));
            }

            Value = value;
        }

        /// <summary>
        ///     floatへの明示的型変換を行う。
        /// </summary>
        /// <param name="health"></param>
        public static explicit operator float(Health health) => health.Value;

        /// <summary>
        ///     等価比較を行う。
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is Health health && Value.Equals(health.Value);
        }

        /// <summary>
        ///     ハッシュ値を計算する。
        /// </summary>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        ///     等価比較を行う。
        /// </summary>
        public bool Equals(Health other)
        {
            return Value.Equals(other.Value);
        }

        /// <summary> 体力値。 </summary>
        public readonly float Value;
    }
}