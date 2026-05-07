using System;
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    /// <summary>
    /// シナリオイベント処理の契約を定義します。
    /// </summary>
    public interface IScenarioEventHandler
    {
        Type EventType { get; }
        ValueTask HandleAsync(IScenarioEvent e, CancellationToken ct);
    }
}
