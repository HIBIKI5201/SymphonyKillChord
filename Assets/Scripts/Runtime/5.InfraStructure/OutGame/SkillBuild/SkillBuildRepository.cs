using KillChord.Runtime.Application.OutGame.SkillBuild;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using System.Collections.Generic;

namespace KillChord.Runtime.InfraStructure.OutGame.SkillBuild
{
    /// <summary>
    ///     プレイヤーの装備スキルに関するデータの永続化や取得を担当するリポジトリの実装クラス。
    ///     TODO : 実際のセーブシステムの実装が完了したら、ここにその処理を追加する必要があります。
    /// </summary>
    public class SkillBuildRepository : ISkillBuildRepository
    {
        public IReadOnlyList<EquippedSkill> GetEquippedSkills()
        {
            throw new System.NotImplementedException();
        }

        public void LoadSkillBuild()
        {
            throw new System.NotImplementedException();
        }

        public void SaveSkillBuild(SkillBuildDefinition skillBuildDefinition)
        {
            throw new System.NotImplementedException();
        }
    }
}
