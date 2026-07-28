using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Character
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

        /// <summary> 攻撃力を取得する。 </summary>
        public float Value => _value;

        /// <summary>
        ///     floatへの明示的型変換を行う。
        /// </summary>
        public static explicit operator float(AttackPower value)
            => value._value;

        /// <summary>
        ///     等価比較を行う。
        /// </summary>
        public static bool operator ==(AttackPower left, AttackPower right)
            => left._value == right._value;

        /// <summary>
        ///     不等価比較を行う。
        /// </summary>
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

        /// <summary>
        ///     等価比較を行う。
        /// </summary>
        public bool Equals(AttackPower other)
            => _value.Equals(other._value);

        /// <summary>
        ///     等価比較を行う。
        /// </summary>
        public override bool Equals(object obj)
            => obj is AttackPower power && Equals(power);

        /// <summary>
        ///     ハッシュ値を計算する。
        /// </summary>
        public override int GetHashCode()
            => _value.GetHashCode();

        private readonly float _value;
    }
}
