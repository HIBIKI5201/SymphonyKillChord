using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    public class ScenarioHandlerRepo
    {
        public void Register<TEvent>(Func<TEvent, CancellationToken, ValueTask<ScenarioHandleResult>> handler)
            where TEvent : IScenarioEvent
        {
            _map[typeof(TEvent)] = (e, token) => handler((TEvent)e, token);
        }

        public ValueTask<ScenarioHandleResult> HandleAsync(IScenarioEvent e, CancellationToken ct)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));

            if (!_map.TryGetValue(e.GetType(), out var handler))
            {
                throw new InvalidOperationException($"Handler not found: {e.GetType().Name}");
            }

            return handler(e, ct);
        }

        private readonly Dictionary<Type, Func<IScenarioEvent, CancellationToken, ValueTask<ScenarioHandleResult>>> _map = new();
    }
}
