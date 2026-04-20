using System;
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Adaptor
{
    public class AnimationEventHandler : IScenarioEventHandler<AnimationEvent>
    {
        public AnimationEventHandler(IAnimationOutputPort animationOutputPort, IAnimationRepository animationRepository)
        {
            _animationOutputPort = animationOutputPort;
            _animationRepository = animationRepository;
        }

        public Type EventType => typeof(AnimationEvent);

        public async ValueTask HandleAsync(AnimationEvent e, CancellationToken ct)
        {
            if (!_animationRepository.TryFindById(e.AnimationId, out AnimationDefinition definition))
            {
                return;
            }

            await _animationOutputPort.PlayAnimationAsync(definition.AssetKey, ct);
        }

        private readonly IAnimationOutputPort _animationOutputPort;
        private readonly IAnimationRepository _animationRepository;
    }
}
