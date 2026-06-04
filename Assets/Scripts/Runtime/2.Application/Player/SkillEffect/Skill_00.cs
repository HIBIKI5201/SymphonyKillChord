using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.Player;
using UnityEngine;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキルの効果をテストするためのクラス。
    /// </summary>
    public class Skill_00 : ISkillEffect
    {
        public void Execute(SkillEffectContext context)
        {
            AttackDefinition attackDefinition = context.PlayerEntity.CombatSpec.GetAttackDefinitionByBeatType(context.CurrentBeatType);
            AttackResult result = AttackCalculator.Calculate(attackDefinition, context.PlayerEntity, context.TargetEntity);
            AttackResult attackResult = new AttackResult(result.FinalDamage * _multiplier, result.IsCritical);

            context.TargetEntity.TakeDamage(attackResult.FinalDamage);

             Debug.Log($"<color=green>Skill_00 を実行しました:{attackResult.FinalDamage}ダメージです。 </color>");
        }

        private float _multiplier = 5f; //ダメージ倍率。
    }
}