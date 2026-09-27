namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Background の表示反映契約を定義する。
    /// </summary>
    public interface IBackgroundViewSink
    {
        /// <summary>
        ///     指定した背景を表示に反映する。
        /// </summary>
        void SetBackground(string assetKey);
    }
}