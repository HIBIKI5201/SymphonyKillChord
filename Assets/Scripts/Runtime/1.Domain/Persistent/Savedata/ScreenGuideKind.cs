using System;

namespace KillChord.Runtime.Domain.Persistent.Savedata
{
    /// <summary>
    ///     アウトゲームの画面を初めて開いたときに出す案内の種類です。
    /// </summary>
    [Flags]
    public enum ScreenGuideKind
    {
        None = 0,
        /// <summary> 作戦画面。 </summary>
        StageSelect = 1 << 0,
        /// <summary> 研究画面。 </summary>
        SkillTree = 1 << 1,
        /// <summary> 改造画面。 </summary>
        SkillBuild = 1 << 2,
        /// <summary> すべての画面。 </summary>
        All = StageSelect | SkillTree | SkillBuild,
    }
}
