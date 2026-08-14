using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;
using KillChord.Runtime.Utility.Persistent;
using UnityEngine;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキルID 00 のスキル効果を実装するクラス。
    /// </summary>
    public class Skill_00 : SkillBase
    {
        public override void Execute(in SkillEffectContext context)
        {
            float multiplier = (float)context.EffectSpec.GetRequiredValue(
                SkillEffectParameterId.DamageMultiplier);
            AttackDefinition attackDefinition = context.PlayerEntity.CombatSpec.GetAttackDefinitionByBeatType(context.CurrentBeatType);


            AttackResult result = AttackCalculator.Calculate(
                attackDefinition,
                context.PlayerEntity,
                context.TargetEntity,
                false, context.
                PlayerEntity.BaseDamage);
            result = new AttackResult(
                result.FinalDamage * multiplier,
                result.IsCritical);

            result = DamageExecutor.Execute(
                context.PlayerEntity,
                context.TargetEntity,
                result,
                DamageAttackType.Skill);
#if UNITY_EDITOR
            Debug.Log($"Skill_00 発動" +
                $"Damage: {result.FinalDamage}," +
                $" Critical: {result.IsCritical}");
#endif
        }
    }
}
