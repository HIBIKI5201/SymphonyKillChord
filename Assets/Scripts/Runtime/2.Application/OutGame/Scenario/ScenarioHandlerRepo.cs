using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Application
{
    public class ScenarioHandlerRepo
    {
        public ScenarioHandlerRepo(IScenarioEventHandler[] scenarioEventHandlers)
        {
            foreach (IScenarioEventHandler handle in scenarioEventHandlers)
            {
                if (_handlers.TryGetValue(handle.EventType, out var result))
                {
                    continue;
                }
                else
                {
                    _handlers[handle.EventType] = handle;
                }
            }
        }
        public IScenarioEventHandler FindById(Type type)
        {
            if (_handlers.TryGetValue(type, out var result))
            {
                return result;
            }
            return null;
        }

        private readonly Dictionary<Type, IScenarioEventHandler> _handlers = new();
    }
}
