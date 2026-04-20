using System.Collections.Generic;


namespace KillChord.Runtime.Domain
{
    public class ScenarioData
    {

        public ScenarioData(IReadOnlyList<IScenarioEvent> events)
        {
            Events = events;
        }
        public IReadOnlyList<IScenarioEvent> Events { get; }
    }
}
