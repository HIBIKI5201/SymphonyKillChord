using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;
using KillChord.Runtime.Utility.Persistent;
using System;
using UnityEngine;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキルID 03 のスキル効果を実装するクラス。
    /// </summary>
    public class Skill_03 : SkillBase
    {
        /// <summary>
        ///     スキル効果を実行します。
        /// </summary>
        /// <param name="context"> 実行コンテキストです。 </param>
        public override void Execute(in SkillEffectContext context)
        {
            if (context.TargetEntity == null)
            {
                return;
            }

            float damageMultiplier = (float)context.EffectSpec.GetRequiredValue(
                SkillEffectParameterId.DamageMultiplier);

            float secondaryDamageRate = (float)context.EffectSpec.GetRequiredValue(
                SkillEffectParameterId.SecondaryDamageRate);

            AttackDefinition attackDefinition =
                context.PlayerEntity.CombatSpec.GetAttackDefinitionByBeatType(context.CurrentBeatType);

            ApplyDamage(context.PlayerEntity, context.TargetEntity, attackDefinition, damageMultiplier, false, context.IsJustHit);

            ReadOnlySpan<CharacterEntity> targets = context.TargetEntities.Span;

            float secondaryDamageMultiplier = damageMultiplier * secondaryDamageRate;

            // 2次ターゲットに対してダメージを適用する
            for (int i = 0; i < targets.Length; i++)
            {
                CharacterEntity target = targets[i];
                if (target == null || target.IsDead || ReferenceEquals(target, context.TargetEntity))
                {
                    continue;
                }

                ApplyDamage(context.PlayerEntity, target, attackDefinition, secondaryDamageMultiplier, true, context.IsJustHit);
            }
        }

        private const float BASE_DAMAGE_MULTIPLIER = 1f;

        private static void ApplyDamage(
            CharacterEntity attacker,
            CharacterEntity defender,
            AttackDefinition attackDefinition,
            float damageMultiplier,
            bool isSecondaryTarget,
            bool isJustHit)
        {
            AttackResult result = AttackCalculator.Calculate(
                attackDefinition,
                attacker,
                defender,
                isJustHit,
                attacker.BaseDamage);

            result = result.WithFinalDamage(result.FinalDamage * damageMultiplier);
            result = DamageExecutor.Execute(attacker, defender, result, DamageAttackType.Skill);

            Debug.Log($"[Skill_03] 発動。" +
                $"Target: {defender}" +
                $"Secondary: {isSecondaryTarget}");
        }
    }
}
