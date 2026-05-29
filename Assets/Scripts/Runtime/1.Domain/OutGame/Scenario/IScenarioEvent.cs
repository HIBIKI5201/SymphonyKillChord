

namespace KillChord.Runtime.Domain.OutGame.Scenario
{
    /// <summary>
    /// シナリオ再生で扱うイベントの共通契約を定義する。
    /// </summary>
    public interface IScenarioEvent
    {
        /// <summary> RequirePlayerAdvance を取得する。 </summary>
        public bool RequirePlayerAdvance { get; }
    }
}