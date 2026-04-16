

using System;
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    public interface IScenarioEventHandler
    {
        public Type EventType { get; }
        public ValueTask HandleAsync(IScenarioEvent e, CancellationToken ct);
    }
}
