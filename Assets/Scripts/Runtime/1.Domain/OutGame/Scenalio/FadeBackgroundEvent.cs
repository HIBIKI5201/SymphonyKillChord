

namespace KillChord.Runtime.Domain
{
    public class FadeBackgroundEvent : IScenalioEvent
    {
        public FadeBackgroundEvent(float duration)
        {
            Duration = duration;
        }

        public float Duration { get; }
    }
}
