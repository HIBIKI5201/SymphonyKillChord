namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    /// IBackgroundViewSink の契約を定義します。
    /// </summary>
    public interface IBackgroundViewSink
    {
        void SetBackground(string assetKey);
    }
}
