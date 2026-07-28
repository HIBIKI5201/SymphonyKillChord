using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Application.OutGame.Scenario;
using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Layer イベントを出力処理へ橋渡しする。
    /// </summary>
    public class LayerEventHandler : IScenarioEventHandler<LayerEvent>
    {
        /// <summary>
        /// レイヤーイベント用の出力先を受け取る。
        /// </summary>
        public LayerEventHandler(ILayerOutputPort layerOutputPort)
        {
            _layerOutputPort = layerOutputPort;
        }

        /// <summary>
        /// 受け取ったイベントを現在の出力先へ反映する。
        /// </summary>
        public ValueTask HandleAsync(LayerEvent e, CancellationToken ct)
        {
            return _layerOutputPort.SetLayerOrderAsync(e.Target.ToString(), e.Order, ct);
        }

        private readonly ILayerOutputPort _layerOutputPort;
    }
}