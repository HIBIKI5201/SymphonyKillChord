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

        /// <summary>
        ///     プレイヤーの装備スキル構成を保存する。
        /// </summary>
        /// <param name="equippedSkills"> 保存する装備スキル構成。 </param>
        Task SaveSkillBuildAsync(IReadOnlyList<EquippedSkill> equippedSkills);

        /// <summary>
        ///     保存記録があるスキルの現在レベル一覧を取得する(スキルID→レベル)。
        ///     記録が無いスキルはこの結果に含まれない。
        /// </summary>
        ValueTask<IReadOnlyDictionary<int, int>> GetSkillLevelsAsync();

        /// <summary>
        ///     指定したスキルのレベルを1上げ、改造Pを1消費する。
        /// </summary>
        /// <param name="skillId"> 対象スキルID。 </param>
        /// <param name="baseLevel"> 保存記録が無い場合の基準レベル(テンプレートの初期レベル)。 </param>
        /// <returns> 改造Pが不足している等の理由で実行できなかった場合は false。 </returns>
        Task<bool> TryLevelUpSkillAsync(int skillId, int baseLevel);
    }
}
