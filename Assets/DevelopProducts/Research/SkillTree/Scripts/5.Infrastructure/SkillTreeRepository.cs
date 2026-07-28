using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DevelopProducts.SkillTree
{
    [CreateAssetMenu(fileName = "SkillTreeRepository", menuName = "DevelopProducts/SkillTree/SkillTreeRepository")]
    public class SkillTreeRepository : ScriptableObject, ISkillTreeRepository
    {
        /// <summary>
        ///     フェーズ毎ノードグループクラス
        /// </summary>
        [Serializable]
        public class SkillPhaseGroup
        {
            /// <summary>フェーズの全てのノード</summary>
            public SkillNodeAsset[] SkillNodeAssets => _skillNodeAssets ?? Array.Empty<SkillNodeAsset>();

            [SerializeField, Tooltip("このフェーズのノード配列")] private SkillNodeAsset[] _skillNodeAssets;
        }

        public SkillNodeEntity[] AllSkillNodes => _nodeDictionary.Values.ToArray();
        public int PhaseCount => _phaseGroups?.Length ?? 0;
        /// <summary>
        ///     ID指定されたNodeEntityを返す
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public SkillNodeEntity GetNode(int id)
        {
            if (!_nodeDictionary.TryGetValue(id, out var skillNode))
                throw new NullReferenceException($"IDが{id}のスキルノードはスキルツリーに存在しません");

            return skillNode;
        }
        /// <summary>
        ///     ID指定されたノードの親を全て返す
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
        /// <summary>
        ///     初期化メソッド
        /// </summary>
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

            NodeStateInitialize();
        }
        /// <summary>
        ///     ノードの状態を初期化する
        /// </summary>
        private void NodeStateInitialize()
        {
            // 最初のフェーズのノードは表示できるようにする
            var originPhase = _phaseGroups[0].SkillNodeAssets;
            foreach (var node in originPhase)
            {
                if (node.SkillNodeEntity.IsOrigin)
                {
                    node.SkillNodeEntity.Unlock();
                }
                node.SkillNodeEntity.NodeEnable();
            }
            //  フェーズのIndexが１以降は非表示にする
            for (int i = 1; i < PhaseCount; i++)
            {
                var phaseNodes = _phaseGroups[i].SkillNodeAssets;
                foreach (var node in phaseNodes)
                {
                    node.SkillNodeEntity.NodeDisable();
                }
            }
        }
        [SerializeField]
        [Tooltip("ノードフェーズ\nIndexが0以外は初期化で見えない")]
        private SkillPhaseGroup[] _phaseGroups;
        private Dictionary<int, SkillNodeEntity> _nodeDictionary;
        private Dictionary<int, SkillNodeEntity[]> _parentsDictionary;
    }
}