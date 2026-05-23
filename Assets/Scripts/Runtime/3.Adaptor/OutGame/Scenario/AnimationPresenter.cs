using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Animation の表示要求を View 側へ橋渡しする。
    /// </summary>
    public sealed class AnimationPresenter : IAnimationOutputPort
    {
        /// <summary>
        /// アニメーション表示の出力先を受け取る。
        /// </summary>
        public AnimationPresenter(IAnimationViewSink viewSink)
        {
            _viewSink = viewSink;
        }

        /// <summary>
        /// アニメーション再生要求をビューへ通知する。
        /// </summary>
        public ValueTask PlayAnimationAsync(string assetKey, CancellationToken ct)
        {
            _viewSink.SetAnimation(assetKey);
            return default;
        }

        private readonly IAnimationViewSink _viewSink;
    }
}