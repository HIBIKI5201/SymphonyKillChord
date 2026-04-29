using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application
{
    public interface IScenarioCompletionNotifier
    {
        ValueTask NotifyCompletedAsync(bool skipped, CancellationToken ct);
    }
}
