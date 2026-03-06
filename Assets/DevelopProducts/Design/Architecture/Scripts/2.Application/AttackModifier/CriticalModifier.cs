using DevelopProducts.Architecture.Domain;
using UnityEngine;

namespace DevelopProducts.Architecture.Application
{
    /// <summary>
    ///     クリティカルダメージを発生させる修飾子。
    /// </summary>
    public class CriticalModifier : IAttackModifier
    {
        /// <summary>
        ///     ダメージを修飾する。
        /// </summary>
        /// <param name="damage"> 元のダメージ。 </param>
        /// <returns> 修飾後のダメージ。 </returns>
        public DamageContext Modify(DamageContext damage)
        {
            if (Random.Range(0f, 1f) <= _criticalChance)
            {
                damage *= _criticalDamage;
            }
            return damage;
        }

        [SerializeField, Range(0, 1), ToolTip("クリティカルが発生する確率(0-1)。")]
        private float _criticalChance = 0.25f;

        [SerializeField, Min(1), ToolTip("クリティカル時のダメージ倍率。")]
        private float _criticalDamage = 2f;
    }
}
