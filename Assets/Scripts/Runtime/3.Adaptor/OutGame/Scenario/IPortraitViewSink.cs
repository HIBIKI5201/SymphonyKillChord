namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Portrait の表示反映契約を定義する。
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