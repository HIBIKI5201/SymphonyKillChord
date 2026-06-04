using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.Player;
using UnityEngine;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///   スキルID 01 のスキル効果を実装するクラス。
    /// </summary>
    public class Skill_01 : ISkillEffect
    {
        public void Execute(SkillEffectContext context)
        {
            Damage damage = new Damage(context.TargetEntity.CurrentHealth.Value * _multiplier);
            context.TargetEntity.TakeDamage(damage);
        }

        private float _multiplier = 0.9f; //強力な一回攻撃。
    }
}