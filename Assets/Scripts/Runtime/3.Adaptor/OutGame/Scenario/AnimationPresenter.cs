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

        public ValueTask PlayAnimationAsync(string animationKey, CancellationToken ct)
        {
            _viewSink.SetAnimation(animationKey);
            return default;
        }

        private readonly IAnimationViewSink _viewSink;
    }
}
