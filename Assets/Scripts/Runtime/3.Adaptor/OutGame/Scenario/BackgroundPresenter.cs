using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor
{
    public sealed class BackgroundPresenter : IBackgroundOutputPort
    {
        public BackgroundPresenter(IBackgroundViewSink viewSink)
        {
            _viewSink = viewSink ?? throw new System.ArgumentNullException(nameof(viewSink));
        }

        public ValueTask ShowBackgroundAsync(string assetKey, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _viewSink.SetBackground(assetKey);
            return default;
        }

        private readonly IBackgroundViewSink _viewSink;
    }
}
