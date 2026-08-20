using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.Player;
using KillChord.Runtime.Domain.InGame.Buff;


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
            Damage damage = new Damage(context.TargetEntity.CurrentHealth.Value * _multiplier);
            context.TargetEntity.TakeDamage(damage);
        }

        private float _multiplier = 0.9f; //強力な一回攻撃。
    }
}
