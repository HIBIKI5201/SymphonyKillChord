using KillChord.Runtime.Application.OutGame.SkillTree;
using KillChord.Runtime.Domain.OutGame.SkillTree;
using KillChord.Runtime.InfraStructure.Repository;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace KillChord.Runtime.InfraStructure.OutGame.SkillTree
{
    /// <summary>
    ///     スキルノードデータを纏めたリポジトリー。
    /// </summary>
    [CreateAssetMenu(fileName = "SkillNodeDataRepo", menuName = "SymphonyDev/SkillTree/SkillNodeDataRepo")]
    public class SkillNodeDataRepo :
        ScriptableObjectRepositoryBase<SkillNodeId, SkillNodeData, SkillNodeData>,
        ISkillNodeRepository
    {
        /// <summary> スキルノード定義Asset一覧です。 </summary>
        public IReadOnlyList<SkillNodeData> SkillNodes => _skillNodes;

        /// <summary>
        ///     スキルノードのIDを指定して、スキルノードデータを取得する。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SkillNodeData FindNodeData(SkillNodeId id)
        {
            if (_skillNodes == null || _skillNodes.Length <= 0)
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

        /// <summary>
        ///     全てのスキルノード定義をDomain Entityとして取得します。
        /// </summary>
        /// <returns> スキルノード定義一覧です。 </returns>
        public IReadOnlyCollection<SkillNodeEntity> GetAll()
        {
            if (_skillNodes == null || _skillNodes.Length == 0)
            {
                return Array.Empty<SkillNodeEntity>();
            }

            List<SkillNodeEntity> result =
                new List<SkillNodeEntity>(_skillNodes.Length);
            for (int i = 0; i < _skillNodes.Length; i++)
            {
                SkillNodeData skillNode = _skillNodes[i];
                if (skillNode == null)
                {
                    continue;
                }

                result.Add(skillNode.ToDomain());
            }

            return result;
        }

        [SerializeField, FormerlySerializedAs("SkillNodes")]
        [Tooltip("スキルノード定義Asset一覧です。")]
        private SkillNodeData[] _skillNodes;

        protected override IReadOnlyList<SkillNodeData> GetEntries() => _skillNodes;

        protected override bool TryBuild(SkillNodeData entry, out SkillNodeId id, out SkillNodeData value)
        {
            id = entry.NodeId;
            value = entry;
            return true;
        }
    }
}
