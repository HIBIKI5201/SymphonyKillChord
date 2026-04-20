using System;
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Adaptor
{
    public class BackgroundEventHandler : IScenarioEventHandler<BackgroundEvent>
    {
        public BackgroundEventHandler(IBackgroundOutputPort backgroundOutputPort, IBackgroundRepository backgroundRepository)
        {
            _backgroundOutputPort = backgroundOutputPort;
            _backgroundRepository = backgroundRepository;
        }

        public Type EventType => typeof(BackgroundEvent);

        public async ValueTask HandleAsync(BackgroundEvent e, CancellationToken ct)
        {
            if (!_backgroundRepository.TryFindById(e.BackgroundId, out BackgroundDefinition definition))
            {
                return;
            }

            await _backgroundOutputPort.ShowBackgroundAsync(definition.AssetKey, ct);
        }

        private readonly IBackgroundOutputPort _backgroundOutputPort;
        private readonly IBackgroundRepository _backgroundRepository;
    }
}
