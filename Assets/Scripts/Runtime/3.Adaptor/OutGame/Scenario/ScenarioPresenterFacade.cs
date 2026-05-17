using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Application.OutGame.Scenario;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// 各種プレゼンターを束ねてシナリオ出力窓口を提供する。
    /// </summary>
    public sealed class ScenarioPresenterFacade : IOutputPort, IScenarioCompletionNotifier
    {
        /// <summary>
        /// 各出力ポートと完了通知先をまとめて受け取る。
        /// </summary>
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

        /// <summary>
        /// テキスト表示要求をビューへ通知する。
        /// </summary>
        public ValueTask ShowTextAsync(string message, CancellationToken ct)
            => _textOutputPort.ShowTextAsync(message, ct);

        /// <summary>
        /// フェード演出要求をビューへ通知する。
        /// </summary>
        public ValueTask FadeAsync(float start, float end, float duration, CancellationToken ct)
            => _fadeOutputPort.FadeAsync(start, end, duration, ct);

        /// <summary>
        /// 背景表示要求をビューへ通知する。
        /// </summary>
        public ValueTask ShowBackgroundAsync(string assetKey, CancellationToken ct)
            => _backgroundOutputPort.ShowBackgroundAsync(assetKey, ct);

        /// <summary>
        /// アニメーション再生要求をビューへ通知する。
        /// </summary>
        public ValueTask PlayAnimationAsync(string assetKey, CancellationToken ct)
            => _animationOutputPort.PlayAnimationAsync(assetKey, ct);

        /// <summary>
        /// 立ち絵表示要求をビューへ通知する。
        /// </summary>
        public ValueTask ShowPortraitAsync(
            string slot,
            string assetKey,
            float positionX,
            float positionY,
            float scale,
            bool visible,
            CancellationToken ct)
            => _portraitOutputPort.ShowPortraitAsync(slot, assetKey, positionX, positionY, scale, visible, ct);

        /// <summary>
        /// レイヤー順変更要求をビューへ通知する。
        /// </summary>
        public ValueTask SetLayerOrderAsync(string target, int order, CancellationToken ct)
            => _layerOutputPort.SetLayerOrderAsync(target, order, ct);

        /// <summary>
        /// シナリオ完了をビューへ通知する。
        /// </summary>
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