using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキルID 05 のスキル効果を実装するクラス。
    /// </summary>
    public class Skill_05 : SkillBase
    {
        public Skill_05(IBuff buff) : base(buff)
        {
        }
        public override void Execute(in SkillEffectContext context)
        {
            float healthCostRatio = (float)context.EffectSpec.GetRequiredValue(
                SkillEffectParameterId.HealthCostRatio);
            float damageMultiplier = (float)context.EffectSpec.GetRequiredValue(
                SkillEffectParameterId.DamageMultiplier);
            Damage selfDamage = new Damage(context.PlayerEntity.CurrentHealth.Value * healthCostRatio);
            context.PlayerEntity.TakeDamage(selfDamage); // _N%消費する。

            Damage attackDamage = new Damage(selfDamage.Value * damageMultiplier); //自身の体力をn%消費して、その消費量×mのダメージを与える。
            context.TargetEntity.TakeDamage(attackDamage);
        }
    }
}
