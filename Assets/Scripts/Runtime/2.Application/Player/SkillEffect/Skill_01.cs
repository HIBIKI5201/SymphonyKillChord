using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;
using KillChord.Runtime.Utility.Persistent;
using UnityEngine;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///   スキルID 01 のスキル効果を実装するクラス。
    /// </summary>
    public class Skill_01 : SkillBase
    {
        public override void Execute(in SkillEffectContext context)
        {
            float damageMultiplier =
                (float)context.EffectSpec.GetRequiredValue(
                    SkillEffectParameterId.DamageMultiplier);

            AttackDefinition attackDefinition =
                context.PlayerEntity.CombatSpec
                    .GetAttackDefinitionByBeatType(context.CurrentBeatType);

            AttackResult result =
                AttackCalculator.Calculate(
                        attackDefinition,
                        context.PlayerEntity,
                        context.TargetEntity,
                        false,
                        context.PlayerEntity.BaseDamage,
                        isCriticalForced: true);

            result =
                result.WithFinalDamage(result.FinalDamage * damageMultiplier);
            result = DamageExecutor.Execute(
                context.PlayerEntity, context.TargetEntity, result, DamageAttackType.Skill);

            Debug.Log($"[Skill_01] 発動" +
                        $"Multiplier: {damageMultiplier}" +
                        $"FinalDamage: {result.FinalDamage}");
        }
    }
}
