using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor
{
    public sealed class AnimationPresenter : IAnimationOutputPort
    {
        public AnimationPresenter(IAnimationViewSink viewSink)
        {
            _viewSink = viewSink;
        }

        public ValueTask PlayAnimationAsync(string assetKey, CancellationToken ct)
        {
            _viewSink.SetAnimation(assetKey);
            return default;
        }

        private readonly IAnimationViewSink _viewSink;
    }
}
