using KillChord.Runtime.Application.OutGame.Scenario;
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    public class FadeEventHandler : IScenarioEventHandler<FadeEvent>
    {
        public FadeEventHandler(IFadeOutputPort fadeOutputPort)
        {
            _fadeOutputPort = fadeOutputPort ?? throw new System.ArgumentNullException(nameof(fadeOutputPort));
        }

        public ValueTask HandleAsync(FadeEvent e, CancellationToken ct)
        {
            return _fadeOutputPort.FadeAsync(e.Start, e.End, e.DurationSec, ct);
        }

        private readonly IFadeOutputPort _fadeOutputPort;
    }
}
