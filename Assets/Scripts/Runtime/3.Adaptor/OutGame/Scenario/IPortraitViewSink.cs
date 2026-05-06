namespace KillChord.Runtime.Adaptor
{
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
