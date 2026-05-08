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
            IPortraitOutputPort portraitOutputPort,
            ILayerOutputPort layerOutputPort,
            IScenarioCompletionViewSink scenarioCompletionViewSink)
        {
            _textOutputPort = textOutputPort;
            _fadeOutputPort = fadeOutputPort;
            _backgroundOutputPort = backgroundOutputPort;
            _animationOutputPort = animationOutputPort;
            _portraitOutputPort = portraitOutputPort;
            _layerOutputPort = layerOutputPort;
            _scenarioCompletionViewSink = scenarioCompletionViewSink;
        }

        public ValueTask ShowTextAsync(string message, CancellationToken ct)
            => _textOutputPort.ShowTextAsync(message, ct);

        public ValueTask FadeAsync(float start, float end, float duration, CancellationToken ct)
            => _fadeOutputPort.FadeAsync(start, end, duration, ct);

        public ValueTask ShowBackgroundAsync(string assetKey, CancellationToken ct)
            => _backgroundOutputPort.ShowBackgroundAsync(assetKey, ct);

        public ValueTask PlayAnimationAsync(string assetKey, CancellationToken ct)
            => _animationOutputPort.PlayAnimationAsync(assetKey, ct);

        public ValueTask ShowPortraitAsync(
            string slot,
            string assetKey,
            float positionX,
            float positionY,
            float scale,
            bool visible,
            CancellationToken ct)
            => _portraitOutputPort.ShowPortraitAsync(slot, assetKey, positionX, positionY, scale, visible, ct);

        public ValueTask SetLayerOrderAsync(string target, int order, CancellationToken ct)
            => _layerOutputPort.SetLayerOrderAsync(target, order, ct);

        public ValueTask NotifyCompletedAsync(bool skipped, CancellationToken ct)
        {
            _scenarioCompletionViewSink.SetScenarioCompleted(skipped);
            return default;
        }

        private readonly ITextOutputPort _textOutputPort;
        private readonly IFadeOutputPort _fadeOutputPort;
        private readonly IBackgroundOutputPort _backgroundOutputPort;
        private readonly IAnimationOutputPort _animationOutputPort;
        private readonly IPortraitOutputPort _portraitOutputPort;
        private readonly ILayerOutputPort _layerOutputPort;
        private readonly IScenarioCompletionViewSink _scenarioCompletionViewSink;
    }
}
