using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.Player;
using UnityEngine;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    public class Skill_03 : ISkillEffect
    {
        public void Execute(SkillEffectContext context)
        {
            AttackDefinition attackDefinition = context.PlayerEntity.CombatSpec.GetAttackDefinitionByBeatType(context.CurrentBeatType);
            AttackResult result = AttackCalculator.Calculate(attackDefinition, context.PlayerEntity, context.TargetEntity,false,context.PlayerEntity.BaseDamage);
            
        }


    }
}
