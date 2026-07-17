using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
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
}
