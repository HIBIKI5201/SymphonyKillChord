using System.Threading;
using System.Threading.Tasks;
namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Layer の表示要求を View 側へ橋渡しする。
    /// </summary>
    public sealed class LayerPresenter : ILayerOutputPort
    {
        /// <summary>
        /// レイヤー表示の出力先を受け取る。
        /// </summary>
        public LayerPresenter(ILayerViewSink viewSink)
        {
            _viewSink = viewSink;
        }

        /// <summary>
        /// レイヤー順変更要求をビューへ通知する。
        /// </summary>
        public ValueTask SetLayerOrderAsync(string target, int order, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _viewSink.SetLayerOrder(target, order);
            return default;
        }

        private readonly ILayerViewSink _viewSink;
    }
}