using KillChord.Runtime.Application.OutGame.SkillTree;
using KillChord.Runtime.Domain.OutGame.SkillTree;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.SkillTree
{
    /// <summary>
    ///     スキルノードデータを纏めたリポジトリー。
    /// </summary>
    [CreateAssetMenu(fileName = "SkillNodeDataRepo", menuName = "SymphonyDev/SkillTree/SkillNodeDataRepo")]
    public class SkillNodeDataRepo : ScriptableObject, ISkillNodeRepository
    {
        public SkillNodeData[] SkillNodes;

        /// <summary>
        ///     全てのスキルノード定義をDomainへ変換して取得します。
        /// </summary>
        /// <returns> スキルノード定義一覧です。 </returns>
        public IReadOnlyCollection<SkillNodeEntity> GetAll()
        {
            if (SkillNodes == null || SkillNodes.Length == 0)
            {
                return new List<SkillNodeEntity>();
            }

            List<SkillNodeEntity> result = new List<SkillNodeEntity>(SkillNodes.Length);
            for (int i = 0; i < SkillNodes.Length; i++)
            {
                if (SkillNodes[i] != null)
                {
                    result.Add(SkillNodes[i].ToDomain());
                }
            }

            return result;
        }

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
            for (int i = 0; i < SkillNodes.Length; i++)
            {
                var node = SkillNodes[i];
                if (node == null)
                {
                    continue;
                }
                if (node.NodeId == id)
                {
                    return node;
                }
            }
            Debug.LogError($"[SkillNodeDataRepo] 指定されてスキルノードIDが見つかりません。");
            return null;
        }

    }
}
