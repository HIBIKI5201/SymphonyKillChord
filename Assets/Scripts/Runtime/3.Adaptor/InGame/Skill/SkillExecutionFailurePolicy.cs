namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     スキル実行失敗時の進捗処理方針です。
    /// </summary>
    public enum SkillExecutionFailurePolicy
    {
        KeepProgress = 0,
        ResetProgressOnly = 1,
        ResetProgressAndConsumeCooldown = 2,
    }
}
