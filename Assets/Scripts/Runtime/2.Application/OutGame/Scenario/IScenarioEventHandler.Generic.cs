using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    /// <summary>
    /// IScenarioEventHandler の契約を定義します。
    /// </summary>
    public interface IScenarioEventHandler<in TEvent> where TEvent : IScenarioEvent
    {
        ValueTask HandleAsync(TEvent e, CancellationToken ct);
    }
}
