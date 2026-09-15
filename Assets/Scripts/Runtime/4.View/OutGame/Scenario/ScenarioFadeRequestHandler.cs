using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Adaptor.OutGame.Scenario;

namespace KillChord.Runtime.View.OutGame.Scenario
{
    /// <summary>
    /// 型付きフェード要求を同期的に受け取り、描画完了まで待機する処理を表す。
    /// </summary>
    public delegate ValueTask ScenarioFadeRequestHandler(
        in ScenarioFadeViewDTO dto,
        CancellationToken cancellationToken);
}
