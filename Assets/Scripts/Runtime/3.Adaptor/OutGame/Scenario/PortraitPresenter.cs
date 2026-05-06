using System.Threading;
using System.Threading.Tasks;
namespace KillChord.Runtime.Adaptor
{
    public sealed class PortraitPresenter : IPortraitOutputPort
    {
        public PortraitPresenter(IPortraitViewSink viewSink)
        {
            _viewSink = viewSink;
        }

        public ValueTask ShowPortraitAsync(
            string slot,
            string assetKey,
            float positionX,
            float positionY,
            float scale,
            bool visible,
            CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _viewSink.SetPortrait(slot, assetKey, positionX, positionY, scale, visible);
            return default;
        }

        private readonly IPortraitViewSink _viewSink;
    }
}
