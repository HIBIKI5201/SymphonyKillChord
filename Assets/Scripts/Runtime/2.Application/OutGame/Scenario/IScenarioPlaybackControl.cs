namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// シナリオ再生操作の契約を定義する。
    /// </summary>
    public interface IScenarioPlaybackControl
    {
        /// <summary>
        ///     早送りの有効・無効を切り替える。
        /// </summary>
        void SetFastForward(bool enabled);
        /// <summary>
        ///     オート送りの有効・無効を切り替える。
        /// </summary>
        void ToggleAutoAdvance();
        /// <summary>
        ///     一時停止と再開を切り替える。
        /// </summary>
        void TogglePause();
        /// <summary>
        ///     シナリオのスキップを要求する。
        /// </summary>
        void RequestSkip();
    }
}