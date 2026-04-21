using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor
{
    public sealed class BackgroundPresenter : IBackgroundOutputPort
    {
        public BackgroundPresenter(IBackgroundViewSink viewSink)
        {
            _viewSink = viewSink;
        }

        public ValueTask ShowBackgroundAsync(string assetKey, CancellationToken ct)
        {
            _viewSink.SetBackground(assetKey);
            return default;
        }

        private readonly IBackgroundViewSink _viewSink;
    }
}
