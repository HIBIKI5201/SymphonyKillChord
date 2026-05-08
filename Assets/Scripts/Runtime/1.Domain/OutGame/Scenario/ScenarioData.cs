using System.Collections.Generic;
using System;


namespace KillChord.Runtime.Domain.OutGame.Scenario
{
    public class ScenarioData
    {

        public ScenarioData(IReadOnlyList<IScenarioEvent> events)
        {
            Events = events ?? throw new ArgumentNullException(nameof(events));
        }
        public IReadOnlyList<IScenarioEvent> Events { get; }
    }
}
