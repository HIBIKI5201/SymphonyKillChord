namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// IScenarioCompletionViewSink の契約を定義します。
    /// </summary>
    public interface IScenarioCompletionViewSink
    {
        void SetScenarioCompleted(bool skipped);
    }
}
