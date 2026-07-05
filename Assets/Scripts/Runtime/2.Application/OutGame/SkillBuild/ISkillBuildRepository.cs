using KillChord.Runtime.Domain.OutGame.SkillBuild;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.OutGame.SkillBuild
{
    /// <summary>
    ///     プレイヤーの装備スキルに関するデータの永続化や取得を担当するリポジトリインターフェース。
    /// </summary>
    public interface ISkillBuildRepository
    {
        /// <summary> プレイヤーの装備スキル構成を読み込む。 </summary>
        ValueTask<IReadOnlyList<EquippedSkill>> LoadSkillBuild();

        /// <summary> プレイヤーの装備スキルのリストを取得する。 </summary>
        ValueTask<IReadOnlyList<EquippedSkill>> GetEquippedSkills();
    }
}
