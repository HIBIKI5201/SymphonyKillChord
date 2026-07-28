namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     スキル実行結果の種別です。
    /// </summary>
    public enum SkillExecutionResultType
    {
        None = 0,
        CooldownBlocked = 1,
        InputProgressed = 2,
        Executed = 3,
        RejectedByTargetPolicy = 4,
    }
}
