using KillChord.Runtime.Domain.InGame.Battle;
using System;

namespace KillChord.Runtime.Application.InGame.Battle
{
    /// <summary>
    ///     クリティカルヒットを処理する攻撃処理ステップ。
    /// </summary>
    [Serializable]
    public class CriticalStep : IAttackStep
    {
        /// <summary>
        ///     攻撃処理ステップを実行する。
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public AttackStepContext Execute(in AttackStepContext context)
        {
            if (!context.IsCriticalForced)
            {
                // 会心率は攻撃者、会心ダメージ倍率は武器（攻撃定義）が持つ。
                float chance = context.Attacker == null ? 0f : context.Attacker.CriticalChance.Value;

                if (UnityEngine.Random.value >= chance)
                {
                    return context;
                }
            }

            // クリティカルダメージ倍率を取得する。
            // もしオーバーライドが指定されていればそれを使用し、そうでなければ攻撃定義の倍率を使用する。
            float criticalDamageMultiplier = context.CriticalDamageMultiplierOverride ??
                context.AttackDefinition.AttackSpec.CriticalMultiplier.Value;

            // クリティカルダメージ倍率は攻撃者のステータス効果によって変化する可能性があるため、
            // 攻撃者が存在する場合はステータス効果を適用する。
            if (context.Attacker != null)
            {
                criticalDamageMultiplier = context.Attacker.StatusEffectSystem
                    .ApplyCriticalDamageMultiplierModifiers(
                        context.Attacker, context.Defender, criticalDamageMultiplier);
            }

            float criticalDamage = context.Damage.Value * criticalDamageMultiplier;
            int nextCriticalCount = context.CriticalCount + 1;

            return new AttackStepContext(new Damage(criticalDamage), nextCriticalCount, context);
        }
    }
}

