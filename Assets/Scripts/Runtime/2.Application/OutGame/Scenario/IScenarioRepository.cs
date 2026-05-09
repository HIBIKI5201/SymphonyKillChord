using KillChord.Runtime.Domain.OutGame.Scenario;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// シナリオデータ取得の契約を定義します。
    /// </summary>
    public interface IScenarioRepository
    {
        ValueTask<ScenarioData> FindByIdAsync(string id, CancellationToken ct);
    }
}
