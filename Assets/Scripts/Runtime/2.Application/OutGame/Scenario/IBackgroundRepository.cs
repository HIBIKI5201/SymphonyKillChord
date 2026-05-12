using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// IBackground の参照情報を取得するリポジトリ。
    /// </summary>
    public interface IBackgroundRepository
    {
        bool TryFindById(string id, out BackgroundDefinition background);
    }
}