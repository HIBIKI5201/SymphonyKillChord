using System;
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    public interface IScenarioEventHandler<in TEvent> where TEvent : IScenarioEvent
    {
        public Type EventType { get; }
        public ValueTask<ScenarioHandleResult> HandleAsync(TEvent e, CancellationToken ct);
    }

    public interface IScenarioEventHandler
    {
        public Type EventType { get; }
        public ValueTask<ScenarioHandleResult> HandleAsync(IScenarioEvent e, CancellationToken ct);
    }

}
