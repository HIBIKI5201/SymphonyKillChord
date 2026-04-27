using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Application;

namespace KillChord.Runtime.Adaptor
{
    public sealed class ScenarioPresenterFacade : IOutputPort, IScenarioCompletionNotifier
    {
        public ScenarioPresenterFacade(
            ITextOutputPort textOutputPort,
            IFadeOutputPort fadeOutputPort,
            IBackgroundOutputPort backgroundOutputPort,
            IAnimationOutputPort animationOutputPort,
            IScenarioCompletionViewSink scenarioCompletionViewSink)
        {
            _textOutputPort = textOutputPort;
            _fadeOutputPort = fadeOutputPort;
            _backgroundOutputPort = backgroundOutputPort;
            _animationOutputPort = animationOutputPort;
            _scenarioCompletionViewSink = scenarioCompletionViewSink;
        }

        public ValueTask ShowTextAsync(string message, CancellationToken ct)
            => _textOutputPort.ShowTextAsync(message, ct);

        public ValueTask FadeAsync(float start, float end, float duration, CancellationToken ct)
            => _fadeOutputPort.FadeAsync(start, end, duration, ct);

        public ValueTask ShowBackgroundAsync(string assetKey, CancellationToken ct)
            => _backgroundOutputPort.ShowBackgroundAsync(assetKey, ct);

        public ValueTask PlayAnimationAsync(string animationId, CancellationToken ct)
            => _animationOutputPort.PlayAnimationAsync(animationId, ct);

        public ValueTask NotifyCompletedAsync(bool skipped, CancellationToken ct)
        {
            _scenarioCompletionViewSink.SetScenarioCompleted(skipped);
            return default;
        }

        private readonly ITextOutputPort _textOutputPort;
        private readonly IFadeOutputPort _fadeOutputPort;
        private readonly IBackgroundOutputPort _backgroundOutputPort;
        private readonly IAnimationOutputPort _animationOutputPort;
        private readonly IScenarioCompletionViewSink _scenarioCompletionViewSink;
    }
}
