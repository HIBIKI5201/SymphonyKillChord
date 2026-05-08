namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// �V�i���I�o�̓|�[�g�̓����_����`���܂��B
    /// </summary>
    public interface IOutputPort
        : ITextOutputPort, IFadeOutputPort, IBackgroundOutputPort, IAnimationOutputPort, IPortraitOutputPort, ILayerOutputPort
    {
    }
}
