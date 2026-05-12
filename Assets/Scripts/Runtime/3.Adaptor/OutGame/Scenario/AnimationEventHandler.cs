using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Application.OutGame.Scenario;
using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    public class AnimationEventHandler : IScenarioEventHandler<AnimationEvent>
    {
        public AnimationEventHandler(IAnimationOutputPort animationOutputPort, IAnimationRepository animationRepository)
        {
            _animationOutputPort = animationOutputPort;
            _animationRepository = animationRepository;
        }

        public async ValueTask HandleAsync(AnimationEvent e, CancellationToken ct)
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
