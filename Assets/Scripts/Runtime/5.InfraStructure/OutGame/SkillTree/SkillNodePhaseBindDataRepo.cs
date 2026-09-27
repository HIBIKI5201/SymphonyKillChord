using KillChord.Runtime.Domain.OutGame.SkillTree;
using KillChord.Runtime.InfraStructure.Repository;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.SkillTree
{
    /// <summary>
    ///     解放段階と必要スキルノードの紐づきを纏めたリポジトリー。
    ///     必要スキルノードIDで引けるよう、他のリポジトリーと同じく辞書で検索する。
    /// </summary>
    [CreateAssetMenu(fileName = "SkillNodePhaseBindDataRepo", menuName = "SymphonyDev/SkillTree/SkillNodePhaseBindDataRepo")]
    public class SkillNodePhaseBindDataRepo : ScriptableObjectRepositoryBase<SkillNodeId, SkillNodePhaseBindData, SkillNodePhaseBindData>
    {
        public SkillNodePhaseBindData[] PhaseBindData;

        /// <summary>
        ///     スキルノードIDを指定し、そのノードの解放で開く段階の名前を取得する。
        /// </summary>
        /// <param name="unlockedNodeId"> 解放したスキルノードのID。 </param>
        /// <param name="phaseName"> 開く段階の名前。見つからない場合はnull。 </param>
        /// <returns> 対応する段階がある場合はtrue。 </returns>
        public bool TryGetUnlockPhaseName(SkillNodeId unlockedNodeId, out string phaseName)
        {
            if (TryFind(unlockedNodeId, out SkillNodePhaseBindData bind))
            {
                phaseName = bind.PhaseName;
                return true;
            }

            phaseName = null;
            return false;
        }

        /// <inheritdoc/>
        protected override IReadOnlyList<SkillNodePhaseBindData> GetEntries() => PhaseBindData;

        /// <inheritdoc/>
        protected override bool TryBuild(SkillNodePhaseBindData entry, out SkillNodeId id, out SkillNodePhaseBindData value)
        {
            value = entry;
            id = entry.RequiredSkillNodeId;
            return true;
        }
    }
}
