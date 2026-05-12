namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// ScenarioCompletion の表示反映契約を定義する。
    /// </summary>
    public interface IScenarioCompletionViewSink
    {
        void SetScenarioCompleted(bool skipped);
    }
}