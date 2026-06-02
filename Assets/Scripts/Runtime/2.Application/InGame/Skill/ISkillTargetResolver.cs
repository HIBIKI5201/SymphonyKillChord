
using KillChord.Runtime.Domain.InGame.Character;
/// <summary>
///    スキルの対象を解決するためのインターフェース。
/// </summary>
public interface ISkillTargetResolver
{
    /// <summary>
    ///     スキルの対象を解決する。
    ///     対象が存在しない場合は null を返す。
    /// </summary>
    /// <returns>スキルの対象となるキャラクターエンティティ。対象が存在しない場合は null。</returns>
    public bool TryGetCurrentTargetEntity(out CharacterEntity entity);
}