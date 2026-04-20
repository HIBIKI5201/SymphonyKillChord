namespace KillChord.Runtime.Adaptor
{
    // Aggregate port used by composition and legacy call sites.
    public interface IOutPutPort : ITextOutputPort, IFadeOutputPort, IBackgroundOutputPort, IAnimationOutputPort
    {
    }
}
