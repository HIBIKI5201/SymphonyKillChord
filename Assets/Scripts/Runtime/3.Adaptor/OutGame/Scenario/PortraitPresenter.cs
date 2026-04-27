using System;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor
{
    public sealed class PortraitPresenter : IPortraitOutputPort
    {
        public PortraitPresenter(IPortraitViewSink viewSink)
        {
            _viewSink = viewSink ?? throw new ArgumentNullException(nameof(viewSink));
        }

        public ValueTask ShowPortraitAsync(string slotId, string assetKey, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _viewSink.SetPortrait(slotId, assetKey);
            return default;
        }

        private readonly IPortraitViewSink _viewSink;
    }
}
