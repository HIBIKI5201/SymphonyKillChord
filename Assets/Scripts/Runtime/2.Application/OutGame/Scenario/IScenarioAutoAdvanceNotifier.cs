namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    ///     シナリオの自動送り状態を表示側へ通知する。
    /// </summary>
    public interface IScenarioAutoAdvanceNotifier
    {
        /// <summary>
        ///     自動送り状態の変更を通知する。
        /// </summary>
        void NotifyAutoAdvanceChanged(bool isAutoAdvance);
    }
}
