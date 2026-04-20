using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    public sealed class AnimationPresenter : IAnimationOutputPort
    {
        public AnimationPresenter(IAnimationViewSink viewSink)
        {
            _viewSink = viewSink;
        }

        public ValueTask PlayAnimationAsync(AnimationClip animationClip, CancellationToken ct)
        {
            _viewSink.SetAnimation(animationClip);
            return default;
        }

        private readonly IAnimationViewSink _viewSink;
    }
}
