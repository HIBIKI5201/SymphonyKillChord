using System;

namespace KillChord.Runtime.Domain
{
    public class BackgroundEvent : IScenarioEvent
    {
        public BackgroundEvent(string backgroundId)
        {
            BackgroundId = string.IsNullOrWhiteSpace(backgroundId)
                ? throw new ArgumentException("backgroundId is empty.", nameof(backgroundId))
                : backgroundId;
        }

        public string BackgroundId { get; }
        public bool RequirePlayerAdvance => false;
    }
}
