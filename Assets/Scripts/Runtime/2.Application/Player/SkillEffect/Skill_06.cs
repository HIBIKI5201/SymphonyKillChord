using KillChord.Runtime.Domain.Player;
using KillChord.Runtime.Domain.InGame.Buff;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
     /// <summary>
    ///     スキルID 06 のスキル効果を実装するクラス。
    /// </summary>
    public class Skill_06 : SkillBase
    {
        public Skill_06(IBuff buff):base(buff)
        {
            _buff = buff;
        }
        public override void Execute(SkillEffectContext context)
        {
            context.PlayerEntity.BuffSystem.Add(_buff);
        }

        private float _m = 5f; // ダメージ倍率。
        private float _n = 0.3f; // 体力消費量。
        private IBuff _buff;
    }
}