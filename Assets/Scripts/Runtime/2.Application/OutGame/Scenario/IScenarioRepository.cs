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
        /// <summary>
        ///     ID に対応するシナリオ定義を非同期で取得する。
        /// </summary>
        ValueTask<ScenarioDefinition> FindByIdAsync(string id, CancellationToken ct);
    }
}

