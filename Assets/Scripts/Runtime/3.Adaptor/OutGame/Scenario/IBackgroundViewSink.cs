namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// IBackgroundViewSink の契約を定義します。
    /// </summary>
    public interface IBackgroundViewSink
    {
        void SetBackground(string assetKey);
    }
}
