namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// ScenarioCompletion の表示反映契約を定義する。
    /// </summary>
    public interface IScenarioCompletionViewSink
    {
        /// <summary>
        /// シナリオ完了状態を反映し、完了時の表示や処理を開始する。
        /// </summary>
        /// <param name="skipped">true の場合はユーザーによるスキップ終了、false の場合は通常完了を示す。</param>
        void SetScenarioCompleted(bool skipped);
    }
}
