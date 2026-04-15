using UnityEngine;

namespace KillChord.Runtime.Domain
{
    public class FadeBackgroudEvent : IScenalioEvent
    {
        public FadeBackgroudEvent(float duration)
        {
            Duration = duration;
        }

        public float Duration { get; }
    }
}
