using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Battle;
using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Battle
{
    /// <summary>
    ///     クリティカルヒットを処理する攻撃処理ステップ。
    /// </summary>
    public class CriticalStep : IAttackStep
    {
        public AttackStepContext Execute(in AttackStepContext context)
        {
            float chance = context.AttackDefinition.AttackParameterSet.CriticalChance.Value;
            float multiplier = context.AttackDefinition.AttackParameterSet.CriticalMultiplier.Value;

            if (Random.value >= chance)
            {
                return context;
            }

            float criticalDamage = context.Damage.Value * multiplier;
            int nextCriticalCount = context.CriticalCount + 1;

            return new AttackStepContext(new Damage(criticalDamage), nextCriticalCount, context);
        }
    }
}
