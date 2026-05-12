

namespace KillChord.Runtime.Domain.OutGame.Scenario
{
    /// <summary>
    /// IScenarioEvent の契約を定義します。
    /// </summary>
    public interface IScenarioEvent
    {
        public bool RequirePlayerAdvance { get; }
    }
}
