using KillChord.Runtime.Domain.InGame.Character;

namespace KillChord.Runtime.Application.InGame.Skill
{
    /// <summary>
    ///     スキルの対象を解決するためのインターフェース。
    /// </summary>
    public interface ISkillTargetResolver
    {
        /// <summary>
        ///     スキルの現在の対象を取得する。
        ///     対象が存在しない場合は false を返す。
        /// </summary>
        /// <param name="entity">   取得された対象エンティティ。存在しない場合はnull </param>
        /// <returns>   対象エンティティが存在する場合は true。</returns>
        public bool TryGetCurrentTargetEntity(out CharacterEntity entity);
    }
}