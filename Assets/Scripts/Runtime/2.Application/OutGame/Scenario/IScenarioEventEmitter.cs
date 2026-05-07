using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    /// <summary>
    /// シナリオイベント発火の契約を定義します。
    /// </summary>
    public interface IScenarioEventEmitter
    {
        ValueTask EmitAsync(IScenarioEvent scenarioEvent, CancellationToken ct);
    }
}
