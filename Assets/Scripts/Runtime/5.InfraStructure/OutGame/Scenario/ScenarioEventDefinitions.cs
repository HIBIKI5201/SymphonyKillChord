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
}
