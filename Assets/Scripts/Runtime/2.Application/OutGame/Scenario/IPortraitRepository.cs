using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// IPortrait の参照情報を取得するリポジトリ。
    /// </summary>
    public interface IPortraitRepository
    {
        bool TryFindById(string id, out PortraitDefinition portrait);
    }
}