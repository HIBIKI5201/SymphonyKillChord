using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DevelopProducts.SkillTree
{
    [CreateAssetMenu(fileName = "SkillTreeRepository", menuName = "DevelopProducts/SkillTree/SkillTreeRepository")]
    public class SkillTreeRepository : ScriptableObject, ISkillTreeRepository
    {
        [Serializable]
        public class SkillPhaseGroup
        {
            public SkillNodeAsset[] SkillNodeAssets => _skillNodeAssets ?? Array.Empty<SkillNodeAsset>();

            [SerializeField] private SkillNodeAsset[] _skillNodeAssets;
        }

        public SkillNodeEntity[] AllSkillNodes => _nodeDictionary.Values.ToArray();

        public SkillNodeEntity GetNode(int id)
        {
            if (!_nodeDictionary.TryGetValue(id, out var skillNode))
                throw new NullReferenceException($"IDが{id}のスキルノードはスキルツリーに存在しません");

            return skillNode;
        }

        public IReadOnlyList<SkillNodeEntity> GetParentNodes(int id)
        {
            if (!_parentsDictionary.TryGetValue(id, out var parentNodes))
                return Array.Empty<SkillNodeEntity>();

            return parentNodes;
        }

        /// <summary>
        ///     指定フェーズのノード一覧を返す
        /// </summary>
        public IReadOnlyList<SkillNodeEntity> GetNodesByPhase(int phase)
        {
            if (phase < 0 || phase >= _phaseGroups.Length)
                return Array.Empty<SkillNodeEntity>();

            return _phaseGroups[phase].SkillNodeAssets
                .Where(a => a?.SkillNodeEntity != null)
                .Select(a => a.SkillNodeEntity)
                .ToArray();
        }
        public void Initialize()
        {
            _nodeDictionary = new Dictionary<int, SkillNodeEntity>();
            _parentsDictionary = new Dictionary<int, SkillNodeEntity[]>();

            IEnumerable<SkillNodeAsset> AllAssets()
            {
                foreach (var group in _phaseGroups)
                {
                    if (group == null) continue;
                    foreach (var asset in group.SkillNodeAssets) yield return asset;
                }
            }

            // Pass1: 全ノードをEntity化
            foreach (var asset in AllAssets())
            {
                if (asset == null) continue;
                asset.ToDomain();
            }

            // Pass2: 親子関係を設定
            foreach (var asset in AllAssets())
            {
                if (asset?.SkillNodeEntity == null) continue;

                var parentAssets = asset.Parents ?? Array.Empty<SkillNodeAsset>();
                var parents = new List<SkillNodeEntity>(parentAssets.Length);
                foreach (var parent in parentAssets)
                {
                    if (parent?.SkillNodeEntity != null)
                        parents.Add(parent.SkillNodeEntity);
                }
                asset.SkillNodeEntity.SetParent(parents.ToArray());
            }

            // Pass3: 辞書構築
            foreach (var asset in AllAssets())
            {
                if (asset?.SkillNodeEntity == null) continue;

                var id = asset.SkillNodeEntity.SkillNodeIdVO.Id;
                _nodeDictionary[id] = asset.SkillNodeEntity;
                _parentsDictionary[id] = asset.SkillNodeEntity.Parents;
            }
        }

        [SerializeField] private SkillPhaseGroup[] _phaseGroups;
        private Dictionary<int, SkillNodeEntity> _nodeDictionary;
        private Dictionary<int, SkillNodeEntity[]> _parentsDictionary;
    }
}