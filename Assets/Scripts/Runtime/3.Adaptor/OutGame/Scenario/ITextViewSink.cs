namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Text の表示反映契約を定義する。
    /// </summary>
    public interface ITextViewSink
    {
        /// <summary>
        ///     テキストの表示内容を反映する。
        /// </summary>
        void SetText(in ScenarioTextViewDTO dto);
    }
}
