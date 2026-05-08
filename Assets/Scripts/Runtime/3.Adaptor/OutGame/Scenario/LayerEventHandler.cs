using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Application.OutGame.Scenario;
using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    public class LayerEventHandler : IScenarioEventHandler<LayerEvent>
    {
        public LayerEventHandler(ILayerOutputPort layerOutputPort)
        {
            _layerOutputPort = layerOutputPort;
        }

        public ValueTask HandleAsync(LayerEvent e, CancellationToken ct)
        {
            return _layerOutputPort.SetLayerOrderAsync(e.Target.ToString(), e.Order, ct);
        }

        private readonly ILayerOutputPort _layerOutputPort;
    }
}
