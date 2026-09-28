namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Portrait の表示反映契約を定義する。
    /// </summary>
    public interface IPortraitViewSink
    {
        /// <summary>
        ///     指定スロットの立ち絵の画像・位置・拡大率・表示状態を反映する。
        /// </summary>
        void SetPortrait(
            string slot,
            string assetKey,
            float positionX,
            float positionY,
            float scale,
            bool visible);
    }
}