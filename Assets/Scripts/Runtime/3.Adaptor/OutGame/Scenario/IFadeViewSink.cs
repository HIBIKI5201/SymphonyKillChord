namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Fade の表示反映契約を定義する。
    /// </summary>
    public interface IFadeViewSink
    {
        void SetFade(string target, float start, float end, float duration);
    }
}