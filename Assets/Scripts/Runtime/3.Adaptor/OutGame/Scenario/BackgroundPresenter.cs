using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Background の表示要求を View 側へ橋渡しする。
    /// </summary>
    public sealed class BackgroundPresenter : IBackgroundOutputPort
    {
        /// <summary>
        /// 背景表示の出力先を受け取る。
        /// </summary>
        public BackgroundPresenter(IBackgroundViewSink viewSink)
        {
            _viewSink = viewSink ?? throw new System.ArgumentNullException(nameof(viewSink));
        }

        /// <summary>
        /// 背景表示要求をビューへ通知する。
        /// </summary>
        public ValueTask ShowBackgroundAsync(string assetKey, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _viewSink.SetBackground(assetKey);
            return default;
        }

        private readonly IBackgroundViewSink _viewSink;
    }
}