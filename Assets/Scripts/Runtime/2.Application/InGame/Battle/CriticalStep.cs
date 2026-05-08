using KillChord.Runtime.Domain.InGame.Battle;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Battle
{
    /// <summary>
    ///     クリティカルヒットを処理する攻撃処理ステップ。
    /// </summary>
    public class CriticalStep : IAttackStep
    {
        /// <summary>
        ///     攻撃処理ステップを実行する。
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
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
