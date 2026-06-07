using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.Player;
using UnityEngine;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキルID 03 のスキル効果を実装するクラス。
    /// </summary>
    public class Skill_03 : ISkillEffect
    {
        public void Execute(SkillEffectContext context)
        {
            AttackDefinition attackDefinition = context.PlayerEntity.CombatSpec.GetAttackDefinitionByBeatType(context.CurrentBeatType);
            AttackResult result = AttackCalculator.Calculate(attackDefinition, context.PlayerEntity, context.TargetEntity, false, context.PlayerEntity.BaseDamage * _damageMultiPlier);
            //ターゲットに対して単発高火力（通常攻撃の2倍くらいの威力）

            if(context.Characters != null) return;

             for(int i = 0 ; i < context.Characters.Length; i++)
             context.Characters[i].TakeDamage(result.FinalDamage / _damageMultiPlier);
            //ターゲットに的中した後、プレイヤーと敵との半直線状にいる敵に対してスキル火力の50％のダメージを与える
        }

        private readonly float _damageMultiPlier = 2f;
    }
}
