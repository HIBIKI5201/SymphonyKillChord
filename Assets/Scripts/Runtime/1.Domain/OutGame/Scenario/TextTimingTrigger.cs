using System;

namespace KillChord.Runtime.Domain
{
    public class TextTimingTrigger
    {
        private TextTimingTrigger(
           TextTriggerKind kind,
           int charIndex,
           string keyword,
           IScenarioEvent fireEvent)
        {
            Kind = kind;
            CharIndex = charIndex;
            Keyword = keyword ?? string.Empty;
            FireEvent = fireEvent ?? throw new ArgumentNullException(nameof(fireEvent));
        }

        public TextTriggerKind Kind { get; }
        public int CharIndex { get; }
        public string Keyword { get; }
        public IScenarioEvent FireEvent { get; }

        public static TextTimingTrigger AtCharIndex(int charIndex, IScenarioEvent fireEvent)
        {
            if (charIndex < 0) throw new ArgumentOutOfRangeException(nameof(charIndex));
            return new TextTimingTrigger(TextTriggerKind.CharIndex, charIndex, string.Empty, fireEvent);
        }

        public static TextTimingTrigger AtKeyword(string keyword, IScenarioEvent fireEvent)
        {
            if (string.IsNullOrWhiteSpace(keyword)) throw new ArgumentException("keyword is empty.", nameof(keyword));
            return new TextTimingTrigger(TextTriggerKind.Keyword, -1, keyword, fireEvent);
        }
    }
    public enum TextTriggerKind
    {
        CharIndex,
        Keyword
    }
}
