using KillChord.Runtime.Application;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using System.Collections.Generic;

namespace KillChord.Runtime.InfraStructure
{
    /// <summary>
    ///     入手済みスキルリポジトリの実装クラス。
    ///     TODO : 実際のセーブシステムの実装が完了したら、ここにその処理を追加する必要があります。
    /// </summary>
    public class OwnedSkillRepository : IOwnedSkillRepository
    {
        public IReadOnlyList<EquippedSkill> GetOwnedSkills()
        {
            throw new System.NotImplementedException();
        }

        public void LoadOwnedSkills()
        {
            throw new System.NotImplementedException();
        }

        public void SaveOwnedSkills(IReadOnlyList<EquippedSkill> ownedSkills)
        {
            throw new System.NotImplementedException();
        }
    }
}
