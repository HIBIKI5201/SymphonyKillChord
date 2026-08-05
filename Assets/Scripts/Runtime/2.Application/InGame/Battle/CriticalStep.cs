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
            // 会心率は攻撃者、会心ダメージ倍率は武器（攻撃定義）が持つ。
            float chance = context.Attacker == null ? 0f : context.Attacker.CriticalChance.Value;
            float multiplier = context.AttackDefinition.AttackSpec.CriticalMultiplier.Value;

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

