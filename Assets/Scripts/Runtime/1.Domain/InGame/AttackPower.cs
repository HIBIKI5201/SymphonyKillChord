using UnityEngine;

namespace KillChord.Runtime.Domain
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
            Value = value;
        }

        public float Value { get; }

        public static explicit operator float(AttackPower value)
            => value.Value;

        public static bool operator ==(AttackPower left, AttackPower right)
            => left.Value == right.Value;

        public static bool operator !=(AttackPower left, AttackPower right)
            => left.Value != right.Value;

        /// <summary>
        ///     加算演算子。
        /// </summary>
        public static AttackPower operator +(AttackPower left, AttackPower right)
            => new(left.Value + right.Value);

        /// <summary>
        ///     減算演算子。
        /// </summary>
        public static AttackPower operator -(AttackPower left, AttackPower right)
            => new(Mathf.Max(left.Value - right.Value,0f));

        /// <summary>
        ///     乗算演算子。
        /// </summary>
        public static AttackPower operator *(AttackPower left, float multiplier)
            => new(left.Value * multiplier);

        public bool Equals(AttackPower other)
            => Value.Equals(other.Value);

        public override bool Equals(object obj)
            => obj is AttackPower power && Equals(power);

        public override int GetHashCode()
            => Value.GetHashCode();
    }
}
