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
    ///     スキルID 05 のスキル効果を実装するクラス。
    /// </summary>
    public class Skill_05 : SkillBase
    {
        public override void Execute(in SkillEffectContext context)
        {
            float healthCostRatio = (float)context.EffectSpec.GetRequiredValue(
                SkillEffectParameterId.HealthCostRatio);
            float damageMultiplier = (float)context.EffectSpec.GetRequiredValue(
                SkillEffectParameterId.DamageMultiplier);

            ValidateParameters(healthCostRatio, damageMultiplier);

            // プレイヤーの最大体力に対する消費体力を計算する
            Health requestHealthCost = new Health(
                context.PlayerEntity.MaxHealth.Value * healthCostRatio);

            // プレイヤーの体力を消費して、スキルのダメージを計算する
            Health consumedHealth = context.PlayerEntity.ConsumeHealth(requestHealthCost);

            AttackDefinition attackDefinition = context.PlayerEntity.CombatSpec
                .GetAttackDefinitionByBeatType(context.CurrentBeatType);

            // スキルのダメージをターゲットに適用する
            var result = AttackCalculator.Calculate(
                attackDefinition,
                context.PlayerEntity,
                context.TargetEntity,
                context.IsJustHit,
                new Damage(consumedHealth.Value),
                applyAttackerModifiers: false,
                applyWeaponDamageMultiplier: false);
            result = result.WithFinalDamage(result.FinalDamage * damageMultiplier);
            result = DamageExecutor.Execute(
                context.PlayerEntity, context.TargetEntity, result, DamageAttackType.Skill);

            Debug.Log(
                $"[Skill05] 発動。" +
                $"RequestedCost:{requestHealthCost.Value:0.##}, " +
                $"ConsumedHealth:{consumedHealth.Value:0.##}, " +
                $"Multiplier:{damageMultiplier:0.##}, " +
                $"FinalDamage:{result.FinalDamage.Value:0.##}, " +
                $"AppliedDamage:{result.AppliedDamage.Value:0.##}");
        }

        /// <summary>
        ///     スキルパラメータを検証します。
        /// </summary>
        private static void ValidateParameters(
            float healthCostRatio,
            float damageMultiplier)
        {
            if (!float.IsFinite(healthCostRatio) ||
                healthCostRatio < 0f ||
                healthCostRatio > 1f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(healthCostRatio),
                    "HP消費率は0以上1以下で指定してください。");
            }

            if (!float.IsFinite(damageMultiplier) ||
                damageMultiplier < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(damageMultiplier),
                    "ダメージ倍率は0以上の有限値で指定してください。");
            }
        }
    }
}
