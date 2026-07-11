using KillChord.Runtime.Domain.Player;

namespace KillChord.Runtime.Application.InGame.Skill
{
    /// <summary>
    ///     スキル効果の実行器インターフェースです。
    /// </summary>
    public interface ISkillEffectExecutor
    {
        /// <summary>
        ///     スキル効果を実行します。
        /// </summary>
        /// <param name="context"> 実行コンテキストです。 </param>
        void Execute(in SkillEffectContext context);
    }
}
