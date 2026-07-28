namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     評価ミッションのHUDに表示する状況を表す列挙体。
    /// </summary>
    public enum MissionEvaluationDisplaySituation
    {
        /// <summary> 失敗した。 </summary>
        Failed = 0,
        /// <summary> 挑戦中。 </summary>
        Challenging = 1,
        /// <summary> 成功した。 </summary>
        Succeeded = 2,
    }
}
