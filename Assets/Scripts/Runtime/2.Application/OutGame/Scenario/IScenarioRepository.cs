using KillChord.Runtime.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application
{
    public interface IScenarioRepository
    {
        ValueTask<ScenarioData> FindByIdAsync(string id, CancellationToken ct);
    }
}
