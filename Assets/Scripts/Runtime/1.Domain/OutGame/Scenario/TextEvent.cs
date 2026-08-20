

using System.Collections.Generic;
using System;

namespace KillChord.Runtime.Domain.OutGame.Scenario
{
    /// <summary>
    /// シナリオ中でテキスト表示を指示するイベント。
    /// </summary>
    public class TextEvent : IScenarioEvent
    {
        /// <summary>
        /// テキストイベントを初期化する。
        /// </summary>
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

        /// <summary> Speaker を取得する。 </summary>
        public string Speaker { get; }
        /// <summary> Text を取得する。 </summary>
        public string Text { get; }

        /// <summary> Triggers を取得する。 </summary>
        public IReadOnlyList<TextTimingTrigger> Triggers { get; }

        /// <summary> RequirePlayerAdvance を取得する。 </summary>
        public bool RequirePlayerAdvance => true;
    }
}