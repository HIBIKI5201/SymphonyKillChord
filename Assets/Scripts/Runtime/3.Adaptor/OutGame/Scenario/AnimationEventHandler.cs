using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Application.OutGame.Scenario;
using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Animation イベントを出力処理へ橋渡しする。
    /// </summary>
    public class AnimationEventHandler : IScenarioEventHandler<AnimationEvent>
    {
        /// <summary>
        /// アニメーションイベント用の依存関係を受け取る。
        /// </summary>
        public AnimationEventHandler(IAnimationOutputPort animationOutputPort, IAnimationRepository animationRepository)
        {
            _animationOutputPort = animationOutputPort;
            _animationRepository = animationRepository;
        }

        /// <summary>
        /// 受け取ったイベントを現在の出力先へ反映する。
        /// </summary>
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