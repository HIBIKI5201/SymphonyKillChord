using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor
{
    public sealed class ScenarioPresenterFacade : IOutPutPort
    {
        public ScenarioPresenterFacade(
            ITextOutputPort textOutputPort,
            IFadeOutputPort fadeOutputPort,
            IBackgroundOutputPort backgroundOutputPort,
            IAnimationOutputPort animationOutputPort)
        {
            _textOutputPort = textOutputPort;
            _fadeOutputPort = fadeOutputPort;
            _backgroundOutputPort = backgroundOutputPort;
            _animationOutputPort = animationOutputPort;
        }

        public ValueTask ShowTextAsync(string message, CancellationToken ct)
            => _textOutputPort.ShowTextAsync(message, ct);

        public ValueTask FadeAsync(float start, float end, float duration, CancellationToken ct)
            => _fadeOutputPort.FadeAsync(start, end, duration, ct);

        public ValueTask ShowBackgroundAsync(string backgroundId, CancellationToken ct)
            => _backgroundOutputPort.ShowBackgroundAsync(backgroundId, ct);

        public ValueTask PlayAnimationAsync(string animationKey, CancellationToken ct)
            => _animationOutputPort.PlayAnimationAsync(animationKey, ct);

        private readonly ITextOutputPort _textOutputPort;
        private readonly IFadeOutputPort _fadeOutputPort;
        private readonly IBackgroundOutputPort _backgroundOutputPort;
        private readonly IAnimationOutputPort _animationOutputPort;
    }
}
