namespace KillChord.Runtime.Domain.OutGame.Scenario
{
    /// <summary>
    /// フェード演出で変化させる表示チャネルを表す。
    /// </summary>
    public enum FadeMode
    {
        /// <summary> CanvasGroup の透明度を変化させる。 </summary>
        Alpha = 0,
        /// <summary> 立ち絵の輪郭と透明度を維持したまま RGB を黒へ変化させる。 </summary>
        Black = 1,
    }
}
