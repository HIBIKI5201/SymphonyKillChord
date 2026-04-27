using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor
{
    public sealed class FadePresenter : IFadeOutputPort
    {
        public FadePresenter(IFadeViewSink viewSink)
        {
            _viewSink = viewSink;
        }

        public ValueTask FadeAsync(float start, float end, float duration, CancellationToken ct)
        {
            _viewSink.SetFade(start, end, duration);
            return default;
        }

        private readonly IFadeViewSink _viewSink;
    }
}
