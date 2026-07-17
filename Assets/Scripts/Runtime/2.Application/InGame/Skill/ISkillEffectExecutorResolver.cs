using KillChord.Runtime.Domain.InGame.Skill;

namespace KillChord.Runtime.Application.InGame.Skill
{
    /// <summary>
    ///     効果定義から実行器を解決するためのインターフェースです。
    /// </summary>
    public interface ISkillEffectExecutorResolver
    {
        /// <summary>
        ///     指定した効果種別に対応する実行器の取得を試みます。
        /// </summary>
        /// <param name="effectType"> 効果種別です。 </param>
        /// <param name="executor"> 取得した実行器です。 </param>
        /// <returns> 取得に成功した場合はtrue。 </returns>
        bool TryResolve(SkillEffectType effectType, out ISkillEffectExecutor executor);
    }
}
