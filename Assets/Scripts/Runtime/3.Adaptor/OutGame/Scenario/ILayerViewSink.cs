namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    /// ILayerViewSink の契約を定義します。
    /// </summary>
    public interface ILayerViewSink
    {
        void SetLayerOrder(string target, int order);
    }
}
