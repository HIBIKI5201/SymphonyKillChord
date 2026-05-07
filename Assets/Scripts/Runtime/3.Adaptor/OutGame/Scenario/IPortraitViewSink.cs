namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    /// IPortraitViewSink の契約を定義します。
    /// </summary>
    public interface IPortraitViewSink
    {
        void SetPortrait(
            string slot,
            string assetKey,
            float positionX,
            float positionY,
            float scale,
            bool visible);
    }
}
