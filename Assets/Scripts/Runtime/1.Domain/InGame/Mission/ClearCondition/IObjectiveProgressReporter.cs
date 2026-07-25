namespace KillChord.Runtime.Domain.InGame.Mission.ClearCondition
{
    /// <summary>
    ///     達成条件の進行状況を数値(現在値/必要値)で報告できることを表すインタフェース。
    /// </summary>
    public interface IObjectiveProgressReporter
    {
        /// <summary>
        ///     現在の達成数を取得します。ステップ開始時からの相対値です。
        /// </summary>
        /// <param name="progress">ミッションの進行状況。</param>
        /// <returns>現在の達成数。</returns>
        int CurrentCount(MissionProgress progress);

        /// <summary> 達成に必要な数です。 </summary>
        int RequiredCount { get; }
    }
}
