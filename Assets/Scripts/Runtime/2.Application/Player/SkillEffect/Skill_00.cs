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
    ///     スキルID 00 のスキル効果を実装するクラス。
    /// </summary>
    public class Skill_00 : SkillBase
    {
        /// <summary>
        ///     対象に、倍率を掛けたスキルダメージを与える。
        /// </summary>
        public override void Execute(in SkillEffectContext context)
        {
            // ダメージ倍率と、現在の拍に対応する攻撃定義を取得する。
            float multiplier = (float)context.EffectSpec.GetRequiredValue(
                SkillEffectParameterId.DamageMultiplier);
            AttackDefinition attackDefinition = context.PlayerEntity.CombatSpec.GetAttackDefinitionByBeatType(context.CurrentBeatType);


            // 武器のダメージ倍率を使わずにダメージを計算し、スキルの倍率を掛ける。
            AttackResult result = AttackCalculator.Calculate(
                attackDefinition,
                context.PlayerEntity,
                context.TargetEntity,
                context.IsJustHit,
                context.PlayerEntity.BaseDamage,
                applyWeaponDamageMultiplier: false);
            result = result.WithFinalDamage(result.FinalDamage * multiplier);

            // ダメージを与える。
            result = DamageExecutor.Execute(
                context.PlayerEntity,
                context.TargetEntity,
                result,
                DamageAttackType.Skill);
#if UNITY_EDITOR
            DevLog.Log($"Skill_00 発動" +
                $"Damage: {result.FinalDamage.Value}," +
                $" Critical: {result.IsCritical}");
#endif
        }
    }
}
