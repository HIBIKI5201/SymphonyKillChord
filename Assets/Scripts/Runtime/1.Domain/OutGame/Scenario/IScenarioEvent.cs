

namespace KillChord.Runtime.Domain
{
    /// <summary>
    /// IScenarioEvent の契約を定義します。
    /// </summary>
    public interface IScenarioEvent
    {
        public bool RequirePlayerAdvance { get; }
    }
}
