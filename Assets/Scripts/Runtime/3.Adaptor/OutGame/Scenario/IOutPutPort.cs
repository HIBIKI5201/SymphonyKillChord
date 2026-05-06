namespace KillChord.Runtime.Adaptor
{
    public interface IOutputPort
        : ITextOutputPort, IFadeOutputPort, IBackgroundOutputPort, IAnimationOutputPort, IPortraitOutputPort, ILayerOutputPort
    {
    }

    [System.Obsolete("Use IOutputPort")]
    public interface IOutPutPort : IOutputPort
    {
    }
}
