using DevelopProducts.Architecture.Domain;
using UnityEngine;

namespace DevelopProducts.Architecture.Application
{
    public class CriticalModifier : IAttackModifier
    {
        public DamageContext Modify(DamageContext damage)
        {
            if (Random.Range(0f, 1f) <= _criticalChance)
            {
                damage *= _criticalDamage;
            }
            return damage;
        }

        [SerializeField]
        private float _criticalChance = 0.25f;
        [SerializeField, Min(1)]
        private float _criticalDamage = 2f;
    }
}
