using System.Collections.Generic;

namespace KillChord.Runtime.Domain
{
    public class ScenalioData
    {
        public ScenalioData(IReadOnlyList<IScenalioEvent> events)
        {
            Events = events;
        }
        public IReadOnlyList<IScenalioEvent> Events { get; }

    }
}
