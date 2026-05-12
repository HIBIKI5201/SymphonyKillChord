namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Background の表示反映契約を定義する。
    /// </summary>
    public interface IBackgroundViewSink
    {
        void SetBackground(string assetKey);
    }
}