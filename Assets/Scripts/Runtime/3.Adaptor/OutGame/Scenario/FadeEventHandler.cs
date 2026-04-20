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

        public async ValueTask<ScenarioHandleResult> HandleAsync(FadeEvent e, CancellationToken ct)
        {
            await _outPort.FadeAsync(e.Start, e.End, e.DurationSec, ct);
            return ScenarioHandleResult.Empty;
        }

        private readonly IOutPutPort _outPort;

    }
}
