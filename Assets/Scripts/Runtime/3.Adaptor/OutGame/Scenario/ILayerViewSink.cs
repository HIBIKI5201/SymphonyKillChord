namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Layer の表示反映契約を定義する。
    /// </summary>
    public interface ILayerViewSink
    {
        /// <summary>
        ///     対象の表示順を変更する。
        /// </summary>
        void SetLayerOrder(string target, int order);
    }
}