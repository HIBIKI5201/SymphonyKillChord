using System;

namespace KillChord.Runtime.Domain
{
    public class FadeEvent : IScenarioEvent
    {
        public FadeEvent(float start, float end, float duration)
        {
            Start = Math.Clamp(start, 0f, 1f);
            End = Math.Clamp(end, 0f, 1f);
            DurationSec = Math.Max(0f, duration);
        }

        public float Start { get; }
        public float End { get; }
        public float DurationSec { get; }
        public bool RequirePlayerAdvance => false;
    }
}
