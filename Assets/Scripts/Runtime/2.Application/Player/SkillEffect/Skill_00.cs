using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;
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
            //  武器なし攻撃を実装するための箱替え。
            AttackDefinition unbulletDefinition = new AttackDefinition(attackDefinition.AttackName, attackDefinition.AttackSpec, attackDefinition.AttackPipeline);
            AttackResult result = AttackCalculator.Calculate(unbulletDefinition, context.PlayerEntity, context.TargetEntity, false, context.PlayerEntity.BaseDamage);
            AttackResult attackResult = new AttackResult(result.FinalDamage * multiplier, result.IsCritical);

            context.TargetEntity.TakeDamage(attackResult.FinalDamage);
#if UNITY_EDITOR
            Debug.Log($"<color=green>Skill_00 を実行しました:{attackResult.FinalDamage}ダメージです。 </color>");
#endif
        }
    }
}
