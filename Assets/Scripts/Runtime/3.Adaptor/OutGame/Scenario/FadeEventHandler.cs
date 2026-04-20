using KillChord.Runtime.Domain;
using System;
using KillChord.Runtime.Application;
using System.Threading.Tasks;
using System.Threading;

namespace KillChord.Runtime.Adaptor
{
    public class FadeEventHandler : IScenarioEventHandler<FadeEvent>
    {
        public FadeEventHandler(IOutPutPort outPutPort)
        {
            _outPort = outPutPort;
        }
        public Type EventType => typeof(FadeEvent);

        public ValueTask HandleAsync(FadeEvent e, CancellationToken ct)
        {
            return _outPort.FadeAsync(e.Start, e.End, e.DurationSec, ct);
        }

        private readonly IOutPutPort _outPort;

    }

    public class BackgroundEventHandler : IScenarioEventHandler<BackgroundEvent>
    {
        public BackgroundEventHandler(IOutPutPort outPutPort, IBackgroundRepository backgroundRepository)
        {
            _outPort = outPutPort;
            _backgroundRepository = backgroundRepository;
        }

        public Type EventType => typeof(BackgroundEvent);

        public async ValueTask HandleAsync(BackgroundEvent e, CancellationToken ct)
        {
            if (!_backgroundRepository.TryFindById(e.BackgroundId, out BackgroundDefinition definition))
            {
                return;
            }

            await _outPort.ShowBackgroundAsync(definition.AssetKey, ct);
        }

        private readonly IOutPutPort _outPort;
        private readonly IBackgroundRepository _backgroundRepository;
    }
}
