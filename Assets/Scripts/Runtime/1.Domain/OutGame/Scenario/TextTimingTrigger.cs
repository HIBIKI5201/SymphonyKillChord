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

        public static TextTimingTrigger AtSuffix(string suffix, IScenarioEvent fireEvent)
        {
            if (string.IsNullOrWhiteSpace(suffix)) throw new ArgumentException("suffix is empty.", nameof(suffix));
            return new TextTimingTrigger(TextTriggerKind.Suffix, -1, suffix, fireEvent);
        }

        public static bool ShouldFire(TextTimingTrigger trigger, int visibleCharCount, string visibleText)
        {
            if (trigger == null) return false;
            return trigger.Kind switch
            {
                TextTriggerKind.CharIndex => trigger.CharIndex == visibleCharCount,
                TextTriggerKind.Keyword => !string.IsNullOrEmpty(trigger.Keyword) &&
                                           !string.IsNullOrEmpty(visibleText) &&
                                           visibleText.Contains(trigger.Keyword, StringComparison.Ordinal),
                TextTriggerKind.Suffix => !string.IsNullOrEmpty(trigger.Keyword) &&
                                          !string.IsNullOrEmpty(visibleText) &&
                                          visibleText.EndsWith(trigger.Keyword, StringComparison.Ordinal),
                _ => false
            };
        }
    }
}
