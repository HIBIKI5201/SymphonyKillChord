using System;
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// IScenario イベントを出力処理へ橋渡しする。
    /// </summary>
    public interface IScenarioEventHandler
    {
        Type EventType { get; }
        ValueTask HandleAsync(IScenarioEvent e, CancellationToken ct);
    }
}