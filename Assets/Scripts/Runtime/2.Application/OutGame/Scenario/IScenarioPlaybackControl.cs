namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// IScenarioPlaybackControl の契約を定義します。
    /// </summary>
    public interface IScenarioPlaybackControl
    {
        void SetFastForward(bool enabled);
        void TogglePause();
        void RequestSkip();
    }
}
