namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// シナリオ再生操作の契約を定義する。
    /// </summary>
    public interface IScenarioPlaybackControl
    {
        void SetFastForward(bool enabled);
        void TogglePause();
        void RequestSkip();
    }
}