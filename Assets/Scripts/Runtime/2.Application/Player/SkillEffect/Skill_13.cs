using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;
using KillChord.Runtime.Utility.Persistent;
using UnityEngine;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキルID 13 のスキル効果を実装するクラス。
    /// </summary>
    public class Skill_13 : SkillBase
    {
        /// <summary>
        ///     スキル効果を実行するメソッド。スキルの効果を対象のキャラクターエンティティに適用する。
        /// </summary>
        /// <param name="context">スキル効果の発動に必要な情報をまとめた構造体。</param>
        public override void Execute(in SkillEffectContext context)
        {
            float damageMultiplier = (float)context.EffectSpec.GetRequiredValue(
                SkillEffectParameterId.DamageMultiplier);

            int attackCount = (int)context.EffectSpec.GetRequiredValue(
                SkillEffectParameterId.HitCount);

            float criticalMultiplier = (float)context.EffectSpec.GetRequiredValue(
                SkillEffectParameterId.CriticalMultiplier);

            AttackDefinition attackDefinition = context.PlayerEntity.CombatSpec
                .GetAttackDefinitionByBeatType(context.CurrentBeatType);

            for (int i = 0; i < attackCount; i++)
            {
                AttackResult result =
                    AttackCalculator.Calculate(attackDefinition,
                    context.PlayerEntity,
                    context.TargetEntity,
                    context.IsJustHit,
                    context.PlayerEntity.BaseDamage,
                    criticalDamageMultiplierOverride: criticalMultiplier);

                result = result.WithFinalDamage(result.FinalDamage * damageMultiplier);
                result = DamageExecutor.Execute(
                    context.PlayerEntity,
                    context.TargetEntity,
                    result,
                    DamageAttackType.Skill);

                Debug.Log($"[Skill_13] 発動　{i + 1}ヒット目" +
                    $"[FinalDamage: {result.FinalDamage}" +
                    $" AppliedDamage: {result.AppliedDamage}," +
                    $"IsCritical: {result.IsCritical}]");


                if (context.TargetEntity.IsDead)
                {
                    break;
                }
            }
        }
    }
}
