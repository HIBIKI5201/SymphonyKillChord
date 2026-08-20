using KillChord.Runtime.Application.InGame.Skill;
using KillChord.Runtime.Domain.Player;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキルの基盤クラス。
    /// </summary>
    public class SkillBase : ISkillEffectExecutor
    {
        /// <summary>
        ///     スキル効果を実行します。
        /// </summary>
        /// <param name="context"> 実行コンテキストです。 </param>
        public virtual void Execute(in SkillEffectContext context)
        {
        }
    }
}
