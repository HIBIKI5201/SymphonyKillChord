using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// IScenarioEventHandler の契約を定義します。
    /// </summary>
    public interface IScenarioEventHandler<in TEvent> where TEvent : IScenarioEvent
    {
        ValueTask HandleAsync(TEvent e, CancellationToken ct);
    }
}
