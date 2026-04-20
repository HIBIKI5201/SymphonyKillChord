using System;
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    public interface IScenarioEventHandler<in TEvent> where TEvent : IScenarioEvent
    {
        Type EventType { get; }
        ValueTask HandleAsync(TEvent e, CancellationToken ct);
    }
}
