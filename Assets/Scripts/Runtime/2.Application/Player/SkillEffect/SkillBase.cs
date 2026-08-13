using KillChord.Runtime.Application.InGame.Skill;
using KillChord.Runtime.Domain.InGame.Buff;
using KillChord.Runtime.Domain.Player;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキルの基盤クラス。
    /// </summary>
    public class SkillBase : ISkillEffectExecutor
    {
        /// <summary>
        ///     状態効果基盤を使用するスキルを初期化します。
        /// </summary>
        protected SkillBase()
        {
        }

        /// <summary>
        ///     スキル基底クラスを初期化します。
        /// </summary>
        /// <param name="buff"> 付与するバフです。 </param>
        protected SkillBase(IBuff buff)
        {
            _buff = buff;
        }

        /// <summary>
        ///     スキル効果を実行します。
        /// </summary>
        /// <param name="context"> 実行コンテキストです。 </param>
        public virtual void Execute(in SkillEffectContext context)
        {
        }

        protected readonly IBuff _buff;
    }
}
