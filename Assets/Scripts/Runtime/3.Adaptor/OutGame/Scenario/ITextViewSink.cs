namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Text の表示反映契約を定義する。
    /// </summary>
    public interface ITextViewSink
    {
        void SetText(string message);
    }
}