using System.Collections.Generic;
using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    /// <summary>
    /// シナリオイベント定義の共通情報を保持する。
    /// </summary>
    internal abstract class EventDefinition
    {
        /// <summary>
        /// イベント定義の共通情報を初期化する。
        /// </summary>
        protected EventDefinition(int step) => Step = step;
        /// <summary> Step を取得する。 </summary>
        public int Step { get; }
        /// <summary>
        /// 保持している定義を実行用イベントへ変換する。
        /// </summary>
        public abstract IScenarioEvent ToEvent();
    }

    /// <summary>
    /// 単体イベントをそのまま保持する定義情報。
    /// </summary>
    internal sealed class PlainEventDefinition : EventDefinition
    {
        /// <summary>
        /// 基底イベント定義のステップ情報を初期化する。
        /// </summary>
        public PlainEventDefinition(int step, IScenarioEvent scenarioEvent) : base(step)
        {
            _scenarioEvent = scenarioEvent;
        }

        /// <summary>
        /// 保持している定義を実行用イベントへ変換する。
        /// </summary>
        public override IScenarioEvent ToEvent() => _scenarioEvent;
        private readonly IScenarioEvent _scenarioEvent;
    }

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