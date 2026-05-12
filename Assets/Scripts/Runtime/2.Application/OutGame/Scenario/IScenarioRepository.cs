using KillChord.Runtime.Domain.OutGame.Scenario;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// IScenario の参照情報を取得するリポジトリ。
    /// </summary>
    public interface IScenarioRepository
    {
        ValueTask<ScenarioData> FindByIdAsync(string id, CancellationToken ct);
    }
}