using System;

namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     ダメージ計算の文脈を保持する構造体。
    /// </summary>
    public readonly struct Damage
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        /// <param name="value"> 初期ダメージ量。 </param>
        public Damage(float value)
        {
            if (!float.IsFinite(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Damage must be finite.");
            }
            if (value < 0) { value = 0; }
            _value = value;
        }

        /// <summary> ダメージ値。 </summary>
        public float Value => _value;

        /// <summary>
        ///     加算演算子。
        /// </summary>
        public static Damage operator +(Damage left, float right)
        {
            return new Damage(left.Value + right);
        }

        /// <summary>
        ///     減算演算子。
        /// </summary>
        public static Damage operator -(Damage left, float right)
        {
            return new Damage(left.Value - right);
        }

        /// <summary>
        ///     乗算演算子。
        /// </summary>
        public static Damage operator *(Damage context, in float multiplier)
        {
            return new Damage(context.Value * multiplier);
        }

        /// <summary>
        ///     除算演算子。
        /// </summary>
        public static Damage operator /(Damage context, in float divisor)
        {
            return new Damage(context.Value / divisor);
        }

        private readonly float _value;
    }
}
