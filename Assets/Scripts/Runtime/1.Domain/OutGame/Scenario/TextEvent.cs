

namespace KillChord.Runtime.Domain
{
    public class TextEvent : IScenarioEvent
    {
        public TextEvent(string speaker, string text)
        {
            Speaker = speaker;
            Text = text;
        }

        public string Speaker { get; }
        public string Text { get; }
    }
}
