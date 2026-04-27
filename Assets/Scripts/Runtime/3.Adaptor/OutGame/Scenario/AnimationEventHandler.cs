using System;
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Application;

namespace KillChord.Runtime.Adaptor
{
    public class AnimationEventHandler : IScenarioEventHandler<KillChord.Runtime.Domain.AnimationEvent>
    {
        public AnimationEventHandler(IAnimationOutputPort animationOutputPort)
        {
            _animationOutputPort = animationOutputPort;
        }

        public Type EventType => typeof(KillChord.Runtime.Domain.AnimationEvent);

        public ValueTask HandleAsync(KillChord.Runtime.Domain.AnimationEvent e, CancellationToken ct)
        {
            return _animationOutputPort.PlayAnimationAsync(e.AnimationId, ct);
        }

        private readonly IAnimationOutputPort _animationOutputPort;
    }
}
