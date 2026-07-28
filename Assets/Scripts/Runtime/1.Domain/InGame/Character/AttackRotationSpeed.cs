using System;

namespace KillChord.Runtime.Domain.InGame.Character
{
    /// <summary>
    ///     攻撃時の回転速度を表す値オブジェクト。
    /// </summary>
    public readonly struct AttackRotationSpeed
    {
        public AttackRotationSpeed(float value)
        {
            if (value < 0f)
            {
                throw new ArgumentException("value must be non-negative.", nameof(value));
            }
            if (!float.IsFinite(value))
            {
                throw new ArgumentException("value must be finite.", nameof(value));
            }
            Value = value;
        }
        /// <summary> 攻撃時の回転速度の値。 </summary>
        public readonly float Value;

        /// <summary> floatへの明示的な型変換を行う。 </summary>
        public static explicit operator float(AttackRotationSpeed v) => v.Value;

        /// <summary> 等価比較を行う。 </summary>
        public override bool Equals(object obj) => obj is AttackRotationSpeed other && other.Value == Value;

        /// <summary> 等価比較を行う。 </summary>
        public bool Equals(AttackRotationSpeed other) => other.Value == Value;

        /// <summary> ハッシュコードを取得する。 </summary>
        public override int GetHashCode() => Value.GetHashCode();

        /// <summary> 等価比較を行う。 </summary>
        public static bool operator ==(AttackRotationSpeed left, AttackRotationSpeed right) => left.Value == right.Value;

        /// <summary> 不等価比較を行う。 </summary>
        public static bool operator !=(AttackRotationSpeed left, AttackRotationSpeed right) => left.Value != right.Value;
    }
}
