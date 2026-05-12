using System.Threading;
using System.Threading.Tasks;
namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    public sealed class LayerPresenter : ILayerOutputPort
    {
        public LayerPresenter(ILayerViewSink viewSink)
        {
            _viewSink = viewSink;
        }

        public ValueTask SetLayerOrderAsync(string target, int order, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _viewSink.SetLayerOrder(target, order);
            return default;
        }

        private readonly ILayerViewSink _viewSink;
    }
}
