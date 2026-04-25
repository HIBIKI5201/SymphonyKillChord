namespace KillChord.Runtime.Application
{
    public interface IScenarioPlaybackControl
    {
        void SetFastForward(bool enabled);
        void TogglePause();
        void RequestSkip();
    }
}
