using System;
using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Character
{
    /// <summary>
    ///     攻撃力を表す値オブジェクト。
    /// </summary>
    public readonly struct AttackPower
    {
        /// <summary>
        ///     攻撃力を初期化するコンストラクタ。
        /// </summary>
        /// <param name="value"></param>
        public AttackPower(float value)
        {
            if (value < 0)
            {
                throw new ArgumentException("value must be non-negative.", nameof(value));
            }
            if (!float.IsFinite(value))
            {
                throw new ArgumentException("Damage must be finite.", nameof(value));
            }

            _value = value;
        }

        public float Value => _value;

        public static explicit operator float(AttackPower value)
            => value._value;

        public static bool operator ==(AttackPower left, AttackPower right)
            => left._value == right._value;

        public static bool operator !=(AttackPower left, AttackPower right)
            => left._value != right._value;

        /// <summary>
        ///     加算演算子。
        /// </summary>
        public static AttackPower operator +(AttackPower left, AttackPower right)
            => new(left._value + right._value);

        /// <summary>
        ///     減算演算子。
        /// </summary>
        public static AttackPower operator -(AttackPower left, AttackPower right)
            => new(Mathf.Max(left._value - right._value, 0f));

        /// <summary>
        ///     乗算演算子。
        /// </summary>
        public static AttackPower operator *(AttackPower left, float multiplier)
            => new(left._value * multiplier);

        public bool Equals(AttackPower other)
            => _value.Equals(other._value);

        public override bool Equals(object obj)
            => obj is AttackPower power && Equals(power);

        public override int GetHashCode()
            => _value.GetHashCode();

        private readonly float _value;
    }
}
