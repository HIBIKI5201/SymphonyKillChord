using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    /// <summary>
    /// アニメーション定義の参照契約を定義します。
    /// </summary>
    public interface IAnimationRepository
    {
        bool TryFindById(string id, out AnimationDefinition animation);
    }
}
