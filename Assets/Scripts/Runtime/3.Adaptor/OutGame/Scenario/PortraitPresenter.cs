using System.Threading;
using System.Threading.Tasks;
namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Portrait の表示要求を View 側へ橋渡しする。
    /// </summary>
    public sealed class PortraitPresenter : IPortraitOutputPort
    {
        /// <summary>
        /// 立ち絵表示の出力先を受け取る。
        /// </summary>
        public PortraitPresenter(IPortraitViewSink viewSink)
        {
            _viewSink = viewSink;
        }

        /// <summary>
        /// 立ち絵表示要求をビューへ通知する。
        /// </summary>
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