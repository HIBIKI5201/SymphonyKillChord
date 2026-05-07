namespace KillChord.Runtime.Application
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
