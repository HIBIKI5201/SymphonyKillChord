using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    public interface IScenarioEventEmitter
    {
        ValueTask EmitAsync(IScenarioEvent scenarioEvent, CancellationToken ct);
    }
}
