using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Adaptor
{
    public class AnimationEventHandler : IScenarioEventHandler<KillChord.Runtime.Domain.AnimationEvent>
    {
        public AnimationEventHandler(IAnimationOutputPort animationOutputPort, IAnimationRepository animationRepository)
        {
            _animationOutputPort = animationOutputPort;
            _animationRepository = animationRepository;
        }

        public async ValueTask HandleAsync(KillChord.Runtime.Domain.AnimationEvent e, CancellationToken ct)
        {
            if (!_animationRepository.TryFindById(e.AnimationId, out AnimationDefinition animation))
            {
                return;
            }

            await _animationOutputPort.PlayAnimationAsync(animation.AssetKey, ct);
        }

        private readonly IAnimationOutputPort _animationOutputPort;
        private readonly IAnimationRepository _animationRepository;
    }
}
