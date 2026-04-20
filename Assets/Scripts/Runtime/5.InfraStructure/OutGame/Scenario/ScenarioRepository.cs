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
            var backgroundStreet = new BackgroundEvent("genki_pose");
            var heroIdle = new AnimationEvent("anim_hero_idle");

            IReadOnlyList<IScenarioEvent> events = new List<IScenarioEvent>
            {
                new TextEvent("misa", "Hello", CreateTriggers(
                    TextTimingTrigger.AtCharIndex(0, _fadeIn),
                    TextTimingTrigger.AtKeyword("danger", backgroundRoom))),
                new TextEvent("misa", "World danger", CreateTriggers(
                    TextTimingTrigger.AtCharIndex(5, heroIdle),
                    TextTimingTrigger.AtKeyword("danger", backgroundStreet))),
                new TextEvent("satoru", "Goodbye", CreateTriggers(
                    TextTimingTrigger.AtCharIndex(1, _fadeOut))),
            };

            return new ScenarioData(events);
        }

        private static IReadOnlyList<TextTimingTrigger> CreateTriggers(params TextTimingTrigger[] triggers)
        {
            if (triggers == null || triggers.Length == 0) return Array.Empty<TextTimingTrigger>();

            var result = new List<TextTimingTrigger>(triggers.Length);
            for (int i = 0; i < triggers.Length; i++)
            {
                if (triggers[i] == null) continue;
                result.Add(triggers[i]);
            }
            return result;
        }

        private readonly FadeEvent _fadeIn = new(0f, 1f, 1.0f);
        private readonly FadeEvent _fadeOut = new(1f, 0f, 1.0f);
    }
}
