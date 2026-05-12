namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// シナリオ再生中の状態参照契約を定義する。
    /// </summary>
    public interface IScenarioPlaybackState
    {
        /// <summary> シナリオ再生が早送り中かを示す。 </summary>
        bool IsFastForward { get; }
        /// <summary> シナリオ再生が一時停止中かを示す。 </summary>
        bool IsPaused { get; }
    }
}
