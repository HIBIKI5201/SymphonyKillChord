namespace KillChord.Runtime.Domain.InGame.Skill
{
    /// <summary>
    ///     スキル発動時の対象解決ルールです。
    /// </summary>
    public enum SkillTargetingType
    {
        None = 0,
        Self = 1,
        CurrentTarget = 2,
        ForwardArea = 3,
        CurrentTargetForwardLine = 4,
    }
}
