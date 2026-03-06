using UnityEngine;

namespace DevelopProducts.Architecture.Domain
{
    /// <summary>
    ///     ダメージ計算の文脈を保持する構造体。
    /// </summary>
    public readonly struct DamageContext
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        /// <param name="value"> 初期ダメージ量。 </param>
        public DamageContext(float value)
        {
            _value = value;
        }

        /// <summary> ダメージ値。 </summary>
        public float Value => _value;

        /// <summary>
        ///     加算演算子。
        /// </summary>
        public static DamageContext operator +(DamageContext left, float right)
        {
            return new DamageContext(left.Value + right);
        }

        /// <summary>
        ///     減算演算子。
        /// </summary>
        public static DamageContext operator -(DamageContext left, float right)
        {
            return new DamageContext(left.Value - right);
        }

        /// <summary>
        ///     乗算演算子。
        /// </summary>
        public static DamageContext operator *(DamageContext context, in float multiplier)
        {
            return new DamageContext(context.Value * multiplier);
        }

        /// <summary>
        ///     除算演算子。
        /// </summary>
        public static DamageContext operator /(DamageContext context, in float divisor)
        {
            return new DamageContext(context.Value / divisor);
        }

        private readonly float _value;
    }
}
