using KillChord.Runtime.Domain.OutGame.SkillBuild;
using System.Collections.Generic;

namespace KillChord.Runtime.Application
{
    /// <summary>
    ///     プレイヤーの取得済みスキルに関するデータの永続化や取得を担当するリポジトリインターフェース。
    /// </summary>
    public interface IOwnedSkillRepository
    {
        /// <summary> プレイヤーの取得済みスキルを読み込む。 </summary>
        void LoadOwnedSkills();

        /// <summary> プレイヤーの取得済みスキルを保存する。 </summary>
        void SaveOwnedSkills(IReadOnlyList<EquippedSkill> ownedSkills);

        /// <summary> プレイヤーの取得済みスキルのリストを取得する。 </summary>
        IReadOnlyList<EquippedSkill> GetOwnedSkills();
    }
}
