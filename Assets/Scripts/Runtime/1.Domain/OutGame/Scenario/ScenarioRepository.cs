using System.Collections.Generic;
using System;

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
              new TextEvent("misa","Hello",CreateTriggers(fade)),
              new TextEvent("misa","World",Array.Empty<TextTimingTrigger>()),
              new TextEvent("satoru","Goodbye",CreateTriggers(fadeOut)),
            };

            return new ScenarioData(events);
        }

        private IReadOnlyList<TextTimingTrigger> CreateTriggers(IScenarioEvent fireEvent)
        {
            if (fireEvent == null) return Array.Empty<TextTimingTrigger>();

            return new List<TextTimingTrigger>
            {
               TextTimingTrigger.AtCharIndex(5, fireEvent)
            };

        }
        private FadeEvent fade = new(0f, 1f, 3f);
        private FadeEvent fadeOut = new(1f, 0f, 3f);
    }
}
