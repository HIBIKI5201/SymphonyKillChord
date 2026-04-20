using System;
using System.Collections.Generic;
using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    public readonly struct ScenarioHandleResult
    {
        public ScenarioHandleResult(IReadOnlyList<IScenarioEvent> emittedEvents)
        {
            EmittedEvents = emittedEvents;
        }

        public IReadOnlyList<IScenarioEvent> EmittedEvents { get; }

        public static ScenarioHandleResult Empty => new ScenarioHandleResult(Array.Empty<IScenarioEvent>());
    }
}
