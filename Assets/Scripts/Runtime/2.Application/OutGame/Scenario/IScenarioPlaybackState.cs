namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// IScenarioPlaybackState の契約を定義します。
    /// </summary>
    public interface IScenarioPlaybackState
    {
        bool IsFastForward { get; }
        bool IsPaused { get; }
    }
}
