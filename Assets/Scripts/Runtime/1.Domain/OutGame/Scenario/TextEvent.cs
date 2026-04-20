

using System.Collections.Generic;
using System;

namespace KillChord.Runtime.Domain
{
    public class TextEvent : IScenarioEvent
    {
        public TextEvent(string speaker, string text, IReadOnlyList<TextTimingTrigger> triggers)
        {
            Speaker = speaker;
            Text = text;
            Triggers = triggers ?? Array.Empty<TextTimingTrigger>();
        }

        public string Speaker { get; }
        public string Text { get; }

        public IReadOnlyList<TextTimingTrigger> Triggers { get; }

        public bool RequirePlayerAdvance => true;
    }
}
