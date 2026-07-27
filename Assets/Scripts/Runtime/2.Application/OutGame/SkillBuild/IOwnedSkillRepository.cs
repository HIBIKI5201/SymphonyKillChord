using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.Utility.OutGame.Savedata;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application
{
    /// <summary>
    ///     プレイヤーの取得済みスキルに関するデータの永続化や取得を担当するリポジトリインターフェース。
    /// </summary>
    public interface IOwnedSkillRepository
    {
        /// <summary> セーブデータシステムを初期化する。デバッグ実装など不要な場合は何もしなくてよい。 </summary>
        void Initialize(SavedataSystem savedataSystem);

        /// <summary> プレイヤーの取得済みスキルを読み込む。 </summary>
        ValueTask<IReadOnlyList<EquippedSkill>> LoadOwnedSkillsAsync();

        /// <summary> プレイヤーの取得済みスキルのリストを取得する。 </summary>
        ValueTask<IReadOnlyList<EquippedSkill>> GetOwnedSkills();
    }
}
