

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
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("text is empty.", nameof(text));

            // 話者名は地の文（ナレーション）用に空文字・空白文字を許可する。
            // null や空白のみの場合は空文字（話者なし）へ正規化する。
            Speaker = string.IsNullOrWhiteSpace(speaker) ? string.Empty : speaker;
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