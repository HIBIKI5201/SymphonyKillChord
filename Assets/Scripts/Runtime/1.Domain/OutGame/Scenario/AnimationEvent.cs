using System;

namespace KillChord.Runtime.Domain
{
    public class AnimationEvent : IScenarioEvent
    {
        public AnimationEvent(string animationId)
        {
            AnimationId = string.IsNullOrWhiteSpace(animationId)
                ? throw new ArgumentException("animationId is empty.", nameof(animationId))
                : animationId;
        }

        public string AnimationId { get; }
        public bool RequirePlayerAdvance => false;
    }
}
