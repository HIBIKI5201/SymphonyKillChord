using KillChord.Runtime.Domain.InGame.Skill;

namespace KillChord.Runtime.Application.InGame.Skill
{
    /// <summary>
    /// スキル定義データを取得するためのリポジトリインターフェース。
    /// </summary>
    public interface ISkillRepository
    {
        /// <summary>
        /// 指定したIDに対応するSkillDefinitionを取得する。
        /// </summary>
        SkillDefinition GetSkill(int id);
    }
}
