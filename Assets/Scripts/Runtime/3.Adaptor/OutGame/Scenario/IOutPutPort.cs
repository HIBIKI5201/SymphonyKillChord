namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// シナリオ演出の出力操作をまとめた契約を定義する。
    /// </summary>
    public interface IOutputPort
        : ITextOutputPort, IFadeOutputPort, IBackgroundOutputPort, IAnimationOutputPort, IPortraitOutputPort, ILayerOutputPort
    {
    }
}