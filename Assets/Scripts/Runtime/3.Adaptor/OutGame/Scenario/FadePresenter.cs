using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Fade の表示要求を View 側へ橋渡しする。
    /// </summary>
    public sealed class FadePresenter : IFadeOutputPort
    {
        /// <summary>
        /// フェード表示の出力先を受け取る。
        /// </summary>
        public FadePresenter(IFadeViewSink viewSink)
        {
            _viewSink = viewSink ?? throw new System.ArgumentNullException(nameof(viewSink));
        }

        /// <summary>
        /// フェード演出要求をビューへ通知する。
        /// </summary>
        public ValueTask FadeAsync(float start, float end, float duration, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _viewSink.SetFade(start, end, duration);
            return default;
        }

        private readonly IFadeViewSink _viewSink;
    }
}