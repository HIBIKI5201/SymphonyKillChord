namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    /// IScenarioCompletionViewSink の契約を定義します。
    /// </summary>
    public interface IScenarioCompletionViewSink
    {
        void SetScenarioCompleted(bool skipped);
    }
}
