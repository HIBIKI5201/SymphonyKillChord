namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// テキスト表示反映の契約を定義します。
    /// </summary>
    public interface ITextViewSink
    {
        void SetText(string message);
    }
}
