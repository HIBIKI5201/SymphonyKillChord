using KillChord.Runtime.Domain.InGame.Battle;
using System;

namespace KillChord.Runtime.Application.InGame.Battle
{
    /// <summary>
    ///     クリティカルヒットを処理する攻撃処理ステップ。
    ///     会心率が100%を超える場合はクリティカルが重なり、ダメージは「クリティカル倍率×クリティカル数」倍になる。
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
            // 会心率は攻撃者、会心ダメージ倍率は武器（攻撃定義）が持つ。
            // 会心率の整数部は確定、小数部は確率でクリティカルの回数が決まる。
            int criticalCount = context.Attacker == null
                ? 0
                : context.Attacker.CriticalChance.RollCriticalCount(UnityEngine.Random.value);

            // 確定クリティカルの場合は、最低1回クリティカルにする。
            if (context.IsCriticalForced)
            {
                criticalCount = Math.Max(criticalCount, 1);
            }

            if (criticalCount <= 0)
            {
                return context;
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

            float criticalDamage = context.Damage.Value * criticalDamageMultiplier * criticalCount;
            int nextCriticalCount = context.CriticalCount + criticalCount;

            return new AttackStepContext(new Damage(criticalDamage), nextCriticalCount, context);
        }
    }
}

