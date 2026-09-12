using System.Threading.Tasks;

namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    ///     シナリオ再生のサービスです。
    /// </summary>
    public interface IScenarioPlaybackService
    {
        /// <summary>
        ///     指定したシナリオを再生します。
        /// </summary>
        /// <param name="scenarioId">再生するシナリオID</param>
        /// <returns>再生完了時に完了する非同期処理です。</returns>
        ValueTask PlayScenario(string scenarioId);

        /// <summary>
        ///     実行中のシナリオをスキップします。
        /// </summary>
        void RequestSkip();
    }
}
