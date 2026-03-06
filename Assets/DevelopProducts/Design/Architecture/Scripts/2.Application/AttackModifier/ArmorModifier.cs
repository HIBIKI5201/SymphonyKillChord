using DevelopProducts.Architecture.Application;
using DevelopProducts.Architecture.Domain;
using UnityEngine;

namespace DevelopProducts.Architecture.Application
{
    /// <summary>
    ///     防具によるダメージ軽減を行う修飾子。
    /// </summary>
    public class ArmorModifier : IAttackModifier
    {
        /// <summary>
        ///     ダメージを修飾する。
        /// </summary>
        /// <param name="damage"> 元のダメージ。 </param>
        /// <returns> 修飾後のダメージ。 </returns>
        public DamageContext Modify(DamageContext damage)
        {
            // ダメージを減らす処理の例
            damage = damage - _armorValue;
            return damage;
        }

        [SerializeField, Tooltip("軽減するダメージの固定値。")]
        private float _armorValue = 10f;
    }
}
