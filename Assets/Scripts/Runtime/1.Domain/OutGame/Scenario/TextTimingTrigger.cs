using System;

namespace KillChord.Runtime.Domain.OutGame.Scenario
{
    /// <summary>
    /// テキスト表示の進行に応じて追加イベントを発火する条件を表す。
    /// </summary>
    public class TextTimingTrigger
    {
        /// <summary>
        /// テキストトリガー条件を初期化する。
        /// </summary>
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

        /// <summary> Kind を取得する。 </summary>
        public TextTriggerKind Kind { get; }
        /// <summary> CharIndex を取得する。 </summary>
        public int CharIndex { get; }
        /// <summary> Keyword を取得する。 </summary>
        public string Keyword { get; }
        /// <summary> FireEvent を取得する。 </summary>
        public IScenarioEvent FireEvent { get; }

        /// <summary>
        /// 指定文字位置で発火するトリガーを生成する。
        /// </summary>
        public static TextTimingTrigger AtCharIndex(int charIndex, IScenarioEvent fireEvent)
        {
            if (charIndex < 0) throw new ArgumentOutOfRangeException(nameof(charIndex));
            return new TextTimingTrigger(TextTriggerKind.CharIndex, charIndex, string.Empty, fireEvent);
        }

        /// <summary>
        /// 指定キーワード到達で発火するトリガーを生成する。
        /// </summary>
        public static TextTimingTrigger AtKeyword(string keyword, IScenarioEvent fireEvent)
        {
            if (string.IsNullOrWhiteSpace(keyword)) throw new ArgumentException("keyword is empty.", nameof(keyword));
            return new TextTimingTrigger(TextTriggerKind.Keyword, -1, keyword, fireEvent);
        }

        /// <summary>
        /// 指定接尾辞の表示完了で発火するトリガーを生成する。
        /// </summary>
        public static TextTimingTrigger AtSuffix(string suffix, IScenarioEvent fireEvent)
        {
            if (string.IsNullOrWhiteSpace(suffix)) throw new ArgumentException("suffix is empty.", nameof(suffix));
            return new TextTimingTrigger(TextTriggerKind.Suffix, -1, suffix, fireEvent);
        }

        /// <summary>
        /// 現在の表示状態でトリガーを発火すべきか判定する。
        /// </summary>
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