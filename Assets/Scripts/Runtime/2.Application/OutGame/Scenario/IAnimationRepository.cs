using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// AnimationDefinition の参照情報を取得するリポジトリ。
    /// </summary>
    public interface IAnimationRepository
    {
        /// <summary>
        ///     ID に対応するアニメーション定義を取得する。見つからない場合は false を返す。
        /// </summary>
        bool TryFindById(AnimationId id, out AnimationDefinition animation);
    }
}
