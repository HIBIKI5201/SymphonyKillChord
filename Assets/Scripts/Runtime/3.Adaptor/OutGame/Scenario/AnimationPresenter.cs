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

        public ValueTask PlayAnimationAsync(string animationId, CancellationToken ct)
        {
            _viewSink.SetAnimation(animationId);
            return default;
        }

        private readonly IAnimationViewSink _viewSink;
    }
}
