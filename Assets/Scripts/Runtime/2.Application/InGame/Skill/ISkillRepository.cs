using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;

namespace KillChord.Runtime.Application.InGame.Skill
{
    /// <summary>
    /// スキル定義データを取得するためのリポジトリインターフェース。
    /// </summary>
    public interface ISkillRepository
    {
        /// <summary>
        /// 指定したIDに対応するSkillDataを取得しようとします。
        /// </summary>
        /// <param name="id"> 取得するスキルの ID。 </param>
        /// <param name="skillData"> 取得したスキルデータ。 </param>
        /// <returns> スキルデータが存在する場合は true、存在しない場合は false。 </returns>
        bool TryGetSkill(int id, out SkillData skillData);

        /// <summary>
        /// 指定したIDに対応するSkillDefinitionを取得する。
        /// </summary>
        SkillDefinition GetSkill(int id, double bpm);
    }
}
