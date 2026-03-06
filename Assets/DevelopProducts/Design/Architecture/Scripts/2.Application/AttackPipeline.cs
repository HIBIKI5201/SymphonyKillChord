using DevelopProducts.Architecture.Domain;
using UnityEngine;

namespace DevelopProducts.Architecture.Application
{
    /// <summary>
    ///     攻撃の計算フローを管理するパイプラインクラス。
    /// </summary>
    public class AttackPipeline
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        /// <param name="attackModifiers"> 適用する攻撃修飾子の配列。 </param>
        public AttackPipeline(IAttackModifier[] attackModifiers)
        {
            _attackModifiers = attackModifiers;
        }

        /// <summary>
        ///     ダメージ計算を処理する。
        /// </summary>
        /// <param name="damage"> 元のダメージ。 </param>
        /// <returns> 計算後のダメージ。 </returns>
        public DamageContext Process(DamageContext damage)
        {
            foreach (IAttackModifier modifier in _attackModifiers)
            {
                damage = modifier.Modify(damage);
            }
            return damage;
        }

        private readonly IAttackModifier[] _attackModifiers;
    }
}
