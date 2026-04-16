using System.Collections.Generic;

namespace KillChord.Runtime.Domain
{
    public class ScenarioRepository : IScenarioRepository
    {
        public ScenarioRepository()
        {

        }

        public ScenarioData FindById(string id)
        {
            List<IScenarioEvent> events = new List<IScenarioEvent>
            {
              new TextEvent("misa","Hello"),
              new TextEvent("misa","World"),
              new TextEvent("satoru","Goodbye"),
            };

            return new ScenarioData(events);
        }
    }
}
