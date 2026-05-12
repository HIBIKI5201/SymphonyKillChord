namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Layer の表示反映契約を定義する。
    /// </summary>
    public interface ILayerViewSink
    {
        void SetLayerOrder(string target, int order);
    }
}