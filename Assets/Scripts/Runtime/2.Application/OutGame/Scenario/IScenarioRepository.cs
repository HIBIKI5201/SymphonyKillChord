using KillChord.Runtime.Domain.OutGame.Scenario;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// ScenarioDefinition の参照情報を取得するリポジトリ。
    /// </summary>
    public interface IScenarioRepository
    {
        ValueTask<ScenarioDefinition> FindByIdAsync(string id, CancellationToken ct);
    }
}

