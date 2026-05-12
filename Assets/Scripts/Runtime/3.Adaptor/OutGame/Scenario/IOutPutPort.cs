namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// シナリオ出力ポートの統合契約を定義します。
    /// </summary>
    public interface IOutputPort
        : ITextOutputPort, IFadeOutputPort, IBackgroundOutputPort, IAnimationOutputPort, IPortraitOutputPort, ILayerOutputPort
    {
    }
}
