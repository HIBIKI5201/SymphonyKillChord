using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.Player;
using UnityEngine;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキルの効果をテストするためのクラス。
    /// </summary>
    public class Skill_05 : ISkillEffect
    {
        public void Execute(SkillEffectContext context)
        {
            Damage selfDamage = new Damage(context.PlayerEntity.CurrentHealth.Value * _n );
            context.PlayerEntity.TakeDamage(selfDamage); // _N%消費する。

            Damage attackDamage = new Damage(selfDamage.Value * _m); //自身の体力をn%消費して、その消費量×mのダメージを与える。
            context.TargetEntity.TakeDamage(attackDamage);
        }

        private float _m = 5f; // ダメージ倍率。
        private float _n = 0.3f; // 体力消費量。
    }
}