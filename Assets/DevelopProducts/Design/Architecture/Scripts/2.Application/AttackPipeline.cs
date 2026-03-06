using DevelopProducts.Architecture.Domain;
using UnityEngine;

namespace DevelopProducts.Architecture.Application
{
    public class AttackPipeline
    {
        public AttackPipeline(IAttackModifier[] attackModifiers)
        {
            _attackModifiers = attackModifiers;
        }

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
