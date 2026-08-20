using KillChord.Runtime.Domain.InGame.Buff;
using KillChord.Runtime.Domain.Player;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキルID 06 のスキル効果を実装するクラス。
    /// </summary>
    public class Skill_06 : SkillBase
    {
        /// <summary>
        ///     スキル効果を初期化します。
        /// </summary>
        /// <param name="buff"> 付与バフです。 </param>
        public Skill_06(IBuff buff) : base(buff)
        {
        }

        /// <summary>
        ///     スキル効果を実行します。
        /// </summary>
        /// <param name="context"> 実行コンテキストです。 </param>
        public override void Execute(in SkillEffectContext context)
        {
            context.PlayerEntity.BuffSystem.Add(_buff);
        }
    }
}
