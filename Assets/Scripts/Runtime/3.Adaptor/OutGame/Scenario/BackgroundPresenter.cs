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

        public ValueTask ShowBackgroundAsync(string backgroundId, CancellationToken ct)
        {
            _viewSink.SetBackground(backgroundId);
            return default;
        }

        private readonly IBackgroundViewSink _viewSink;
    }
}
