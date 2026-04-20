using System;
using System.Collections.Generic;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.InfraStructure
{
    public class ScenarioRepository : IScenarioRepository
    {
        public ScenarioData FindById(string id)
        {
            var backgroundRoom = new BackgroundEvent("bg_room");
            var backgroundStreet = new BackgroundEvent("bg_street");

            IReadOnlyList<IScenarioEvent> events = new List<IScenarioEvent>
            {
                new TextEvent("misa", "Hello", CreateTriggers(_fadeIn, backgroundRoom)),
                new TextEvent("misa", "World danger", CreateTriggers(_fadeIn, backgroundStreet)),
                new TextEvent("satoru", "Goodbye", CreateTriggers(_fadeOut)),
            };

            return new ScenarioData(events);
        }

        private static IReadOnlyList<TextTimingTrigger> CreateTriggers(IScenarioEvent charIndexEvent, IScenarioEvent keywordEvent = null)
        {
            var triggers = new List<TextTimingTrigger>();

            if (charIndexEvent != null)
            {
                triggers.Add(TextTimingTrigger.AtCharIndex(5, charIndexEvent));
            }

            if (keywordEvent != null)
            {
                triggers.Add(TextTimingTrigger.AtKeyword("danger", keywordEvent));
            }

            return triggers;
        }

        private readonly FadeEvent _fadeIn = new(0f, 1f, 1.0f);
        private readonly FadeEvent _fadeOut = new(1f, 0f, 1.0f);
    }
}
