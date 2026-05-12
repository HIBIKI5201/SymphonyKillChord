namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// シナリオ再生中の状態参照契約を定義する。
    /// </summary>
    public interface IScenarioPlaybackState
    {
        bool IsFastForward { get; }
        bool IsPaused { get; }
    }
}