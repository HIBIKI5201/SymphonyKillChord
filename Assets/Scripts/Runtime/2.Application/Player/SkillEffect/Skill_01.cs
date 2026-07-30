using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;


namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///   スキルID 01 のスキル効果を実装するクラス。
    /// </summary>
    public class Skill_01 : SkillBase
    {
        public Skill_01(IBuff buff) : base(buff)
        {
        }
        public override void Execute(in SkillEffectContext context)
        {
            float currentHealthRatio = (float)context.EffectSpec.GetRequiredValue(
                SkillEffectParameterId.CurrentHealthRatio);
            Damage damage = new Damage(context.TargetEntity.CurrentHealth.Value * currentHealthRatio);
            context.TargetEntity.TakeDamage(damage);
        }
    }
}
