using System;
using KillChord.Runtime.Application;
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Adaptor
{
    public class FadeEventHandler : IScenarioEventHandler<FadeEvent>
    {
        public FadeEventHandler(IFadeOutputPort fadeOutputPort)
        {
            _fadeOutputPort = fadeOutputPort;
        }
        public Type EventType => typeof(FadeEvent);

        public ValueTask HandleAsync(FadeEvent e, CancellationToken ct)
        {
            return _fadeOutputPort.FadeAsync(e.Start, e.End, e.DurationSec, ct);
        }

        private readonly IFadeOutputPort _fadeOutputPort;
    }
}
