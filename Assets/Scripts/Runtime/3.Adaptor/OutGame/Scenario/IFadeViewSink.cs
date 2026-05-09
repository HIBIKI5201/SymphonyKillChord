namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// IFadeViewSink の契約を定義します。
    /// </summary>
    public interface IFadeViewSink
    {
        void SetFade(float start, float end, float duration);
    }
}
