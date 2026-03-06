using DevelopProducts.Architecture.Application;
using DevelopProducts.Architecture.Domain;
using UnityEngine;

namespace DevelopProducts.Architecture.Application
{
    public class ArmorModifier : IAttackModifier
    {
        public DamageContext Modify(DamageContext damage)
        {
            // ダメージを減らす処理の例
            damage = damage - _armorValue;
            return damage;
        }

        [SerializeField]
        private float _armorValue = 10f;
    }
}
