using KillChord.Runtime.Domain.InGame.Skill;

namespace KillChord.Runtime.Application.InGame.Skill
{
    /// <summary>
    ///     スキル対象の解決を行うインターフェースです。
    /// </summary>
    public interface ISkillTargetResolver
    {
        /// <summary>
        ///     スキル対象の解決を試みます。
        /// </summary>
        /// <param name="targetingType"> 対象解決ルールです。 </param>
        /// <param name="result"> 解決結果です。 </param>
        /// <returns> 解決に成功した場合はtrue。 </returns>
        bool TryResolveTargets(SkillTargetingType targetingType, out SkillTargetResolveResult result);
    }
}
