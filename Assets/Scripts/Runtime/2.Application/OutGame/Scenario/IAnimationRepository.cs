using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// アニメーション定義の参照契約を定義します。
    /// </summary>
    public interface IAnimationRepository
    {
        bool TryFindById(string id, out AnimationDefinition animation);
    }
}
