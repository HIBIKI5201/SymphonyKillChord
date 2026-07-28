using KillChord.Runtime.Domain.OutGame.SkillTree;
using KillChord.Runtime.InfraStructure.Repository;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.SkillTree
{
    /// <summary>
    ///     スキルノードデータを纏めたリポジトリー。
    /// </summary>
    [CreateAssetMenu(fileName = "SkillNodeDataRepo", menuName = "SymphonyDev/SkillTree/SkillNodeDataRepo")]
    public class SkillNodeDataRepo : ScriptableObjectRepositoryBase<SkillNodeId, SkillNodeData, SkillNodeData>
    {
        public SkillNodeData[] SkillNodes;

        /// <summary>
        ///     スキルノードのIDを指定して、スキルノードデータを取得する。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SkillNodeData FindNodeData(SkillNodeId id)
        {
            if (SkillNodes == null || SkillNodes.Length <= 0)
            {
                Debug.LogError($"[SkillNodeDataRepo] スキルノード情報リポジトリーが空です。");
                return null;
            }

            if (!TryFind(id, out SkillNodeData node))
            {
                Debug.LogError($"[SkillNodeDataRepo] 指定されてスキルノードIDが見つかりません。");
                return null;
            }

            return node;
        }

        protected override IReadOnlyList<SkillNodeData> GetEntries() => SkillNodes;

        protected override bool TryBuild(SkillNodeData entry, out SkillNodeId id, out SkillNodeData value)
        {
            id = entry.NodeId;
            value = entry;
            return true;
        }
    }
}
