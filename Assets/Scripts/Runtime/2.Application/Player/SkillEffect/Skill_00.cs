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
            Damage damage =  new Damage(context.PlayerBaseAttackPower * 5f); //基礎攻撃力の500%に相当するダメージを計算する例
            context.TargetEntity.TakeDamage(damage); //ターゲットに基礎攻撃力の10%に相当するダメージを与える例
            Debug.Log($"<color=green>Skill_00 を実行しました:{damage.Value}ダメージです。 </color>");
        }
    }
}