namespace KillChord.Runtime.Application
{
    public interface IScenarioPlaybackState
    {
        bool IsFastForward { get; }
        bool IsPaused { get; }
    }
}
