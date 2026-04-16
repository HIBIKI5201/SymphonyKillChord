using System.Collections.Generic;


namespace KillChord.Runtime.Domain
{
    public class ScenarioData
    {
        public ScenarioData(List<IScenarioEvent> events)
        {
            Events = events;
        }
        public List<IScenarioEvent> Events { get; }
    }
}
