namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     サブミッションを表示するタイミングを表す列挙型。
    /// </summary>
    public enum MissionEvaluationResultTiming
    {
        /// <summary> 条件達成と同時に表示。 </summary>
        Immediate = 0,
        /// <summary> ミッション終了後に表示。 </summary>
        Cleared = 1,
    }
}
