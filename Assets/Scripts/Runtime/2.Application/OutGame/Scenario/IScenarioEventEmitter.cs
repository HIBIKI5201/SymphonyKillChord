using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// シナリオイベントの再送出契約を定義する。
    /// </summary>
    public interface IScenarioEventEmitter
    {
        ValueTask EmitAsync(IScenarioEvent scenarioEvent, CancellationToken ct);
    }
}