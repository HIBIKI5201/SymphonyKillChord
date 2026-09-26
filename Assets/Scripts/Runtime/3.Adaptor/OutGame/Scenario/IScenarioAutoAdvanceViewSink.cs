namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    ///     自動送り状態をシナリオ表示モデルへ渡す。
    /// </summary>
    public interface IScenarioAutoAdvanceViewSink
    {
        /// <summary>
        ///     表示用の自動送り状態を更新する。
        /// </summary>
        void SetAutoAdvance(bool isAutoAdvance);
    }
}
