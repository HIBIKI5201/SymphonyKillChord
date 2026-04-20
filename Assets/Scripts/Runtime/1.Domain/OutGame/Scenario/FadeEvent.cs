using System;

namespace KillChord.Runtime.Domain
{
    public class FadeEvent : IScenarioEvent
    {
        public FadeEvent(float start, float end, float duration)
        {
            Start = Math.Clamp(start, 0f, 1f);
            End = Math.Clamp(end, 0f, 1f);
            DurationSec = duration;
        }

        public float Start { get; }
        public float End { get; }
        public float DurationSec { get; }
        public bool RequirePlayerAdvance => false;
    }

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
