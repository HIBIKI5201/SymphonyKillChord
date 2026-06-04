using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.Player;
using UnityEngine;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     プレイヤーのスキル効果を表すクラス。スキルID 13に対応する。
    /// </summary>
    public class Skill_13 : ISkillEffect
    {
        /// <summary>
        ///     スキル効果を実行するメソッド。スキルの効果を対象のキャラクターエンティティに適用する。
        /// </summary>
        /// <param name="context">スキル効果の発動に必要な情報をまとめた構造体。</param>
        public void Execute(SkillEffectContext context)
        {
            AttackDefinition attackDefinition = context.PlayerEntity.CombatSpec.GetAttackDefinitionByBeatType(context.CurrentBeatType);
            AttackResult result = AttackCalculator.Calculate(attackDefinition, context.PlayerEntity, context.TargetEntity);
            AttackResult attackResult = new AttackResult(result.FinalDamage * _multiplier, result.IsCritical);

            for(int i = 0; i < _attackCount; i++)
            {
                context.TargetEntity.TakeDamage(attackResult.FinalDamage);
                 Debug.Log($"<color=green>{i + 1} 回目の、Skill_13 を実行しました:{attackResult.FinalDamage}ダメージです。 </color>");
            }
            
        }
 
        private float _multiplier = 0.6f; //ダメージ倍率。
        private int _attackCount = 5;
    }
}