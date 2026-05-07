using KillChord.Runtime.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application
{
    /// <summary>
    /// シナリオデータ取得の契約を定義します。
    /// </summary>
    public interface IScenarioRepository
    {
        ValueTask<ScenarioData> FindByIdAsync(string id, CancellationToken ct);
    }
}
