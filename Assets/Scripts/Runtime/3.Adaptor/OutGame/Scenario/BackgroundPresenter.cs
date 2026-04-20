using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    public sealed class BackgroundPresenter : IBackgroundOutputPort
    {
        public BackgroundPresenter(IBackgroundViewSink viewSink)
        {
            _viewSink = viewSink;
        }

        public ValueTask ShowBackgroundAsync(Sprite background, CancellationToken ct)
        {
            _viewSink.SetBackground(background);
            return default;
        }

        private readonly IBackgroundViewSink _viewSink;
    }
}
