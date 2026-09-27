using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;
using KillChord.Runtime.Utility.Diagnostics;
using KillChord.Runtime.Utility.Persistent;
using UnityEngine;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///   スキルID 01 のスキル効果を実装するクラス。
    /// </summary>
    public class Skill_01 : SkillBase
    {
        /// <summary>
        ///     対象に、必ず会心になる倍率付きのスキルダメージを与える。
        /// </summary>
        public override void Execute(in SkillEffectContext context)
        {
            // ダメージ倍率と、現在の拍に対応する攻撃定義を取得する。
            float damageMultiplier =
                (float)context.EffectSpec.GetRequiredValue(
                    SkillEffectParameterId.DamageMultiplier);

            AttackDefinition attackDefinition =
                context.PlayerEntity.CombatSpec
                    .GetAttackDefinitionByBeatType(context.CurrentBeatType);

            // 必ず会心になるようにダメージを計算し、スキルの倍率を掛ける。
            AttackResult result =
                AttackCalculator.Calculate(
                        attackDefinition,
                        context.PlayerEntity,
                        context.TargetEntity,
                        context.IsJustHit,
                        context.PlayerEntity.BaseDamage,
                        isCriticalForced: true);

            result =
                result.WithFinalDamage(result.FinalDamage * damageMultiplier);
            // ダメージを与える。
            result = DamageExecutor.Execute(
                context.PlayerEntity, context.TargetEntity, result, DamageAttackType.Skill);

            DevLog.Log($"[Skill_01] 発動" +
                        $"Multiplier: {damageMultiplier}" +
                        $"FinalDamage: {result.FinalDamage.Value}");
        }
    }
}
