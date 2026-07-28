using KillChord.Runtime.Domain.OutGame.Scenario;
using System.Collections.Generic;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    /// <summary>
    /// テキストイベントと付随トリガーを組み立てる定義情報。
    /// </summary>
    internal sealed class TextEventDefinition : EventDefinition
    {
        /// <summary>
        /// 基底イベント定義のステップ情報を初期化する。
        /// </summary>
        public TextEventDefinition(int step, string speaker, string text) : base(step)
        {
            Speaker = speaker;
            Text = text;
        }

        /// <summary> Speaker を取得する。 </summary>
        public string Speaker { get; }

        /// <summary> Text を取得する。 </summary>
        public string Text { get; }

        /// <summary>
        /// テキストイベント定義にトリガーを追加する。
        /// </summary>
        public void AddTrigger(TextTimingTrigger trigger) => _triggers.Add(trigger);

        /// <summary>
        /// 保持している定義を実行用イベントへ変換する。
        /// </summary>
        public override IScenarioEvent ToEvent()
        {
            _cached ??= new TextEvent(Speaker, Text, _triggers.ToArray());
            return _cached;
        }

        private readonly List<TextTimingTrigger> _triggers = new();
        private IScenarioEvent _cached;
    }
}
