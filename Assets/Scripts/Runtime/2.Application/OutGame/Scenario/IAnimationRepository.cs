using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// IAnimation の参照情報を取得するリポジトリ。
    /// </summary>
    public interface IAnimationRepository
    {
        bool TryFindById(string id, out AnimationDefinition animation);
    }
}