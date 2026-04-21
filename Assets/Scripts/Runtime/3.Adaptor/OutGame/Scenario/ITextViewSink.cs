namespace KillChord.Runtime.Adaptor
{
    public interface ITextViewSink
    {
        void SetText(string message);
    }

    public interface IScenarioCompletionViewSink
    {
        void SetScenarioCompleted(bool skipped);
    }
}
