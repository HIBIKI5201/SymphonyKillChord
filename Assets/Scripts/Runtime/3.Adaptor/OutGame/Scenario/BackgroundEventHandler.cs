using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Application.OutGame.Scenario;
using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    public class BackgroundEventHandler : IScenarioEventHandler<BackgroundEvent>
    {
        public BackgroundEventHandler(IBackgroundOutputPort backgroundOutputPort, IBackgroundRepository backgroundRepository)
        {
            _backgroundOutputPort = backgroundOutputPort;
            _backgroundRepository = backgroundRepository;
        }

        public async ValueTask HandleAsync(BackgroundEvent e, CancellationToken ct)
        {
            if (!_backgroundRepository.TryFindById(e.BackgroundId, out BackgroundDefinition background))
            {
                return;
            }

            await _backgroundOutputPort.ShowBackgroundAsync(background.AssetKey, ct);
        }

        private readonly IBackgroundOutputPort _backgroundOutputPort;
        private readonly IBackgroundRepository _backgroundRepository;
    }
}
