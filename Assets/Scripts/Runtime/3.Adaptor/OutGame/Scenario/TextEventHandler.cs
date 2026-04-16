using System;
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Adaptor
{
    public class TextEventHandler : IScenarioEventHandler
    {
        public TextEventHandler(IOutPutPort outPutPort)
        {
            _outPort = outPutPort;
        }
        public Type EventType => typeof(TextEvent);

        public async ValueTask HandleAsync(IScenarioEvent e, CancellationToken ct)
        {
            await _outPort.ShowTextAsync(ct);
        }

        private readonly IOutPutPort _outPort;

    }
}
