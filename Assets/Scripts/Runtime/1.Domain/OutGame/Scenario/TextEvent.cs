

using System.Collections.Generic;
using System;

namespace KillChord.Runtime.Domain
{
    public class TextEvent : IScenarioEvent
    {
        public TextEvent(string speaker, string text, IReadOnlyList<TextTimingTrigger> triggers)
        {
            if (speaker == null) throw new ArgumentNullException(nameof(speaker));
            if (string.IsNullOrWhiteSpace(speaker)) throw new ArgumentException("speaker is empty.", nameof(speaker));
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("text is empty.", nameof(text));

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
