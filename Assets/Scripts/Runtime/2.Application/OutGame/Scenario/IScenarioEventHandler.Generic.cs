using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// TEvent として受け取る IScenarioEvent を出力処理へ橋渡しする。
    /// </summary>
    public interface IScenarioEventHandler<in TEvent> where TEvent : IScenarioEvent
    {
        ValueTask HandleAsync(TEvent e, CancellationToken ct);
    }
}
