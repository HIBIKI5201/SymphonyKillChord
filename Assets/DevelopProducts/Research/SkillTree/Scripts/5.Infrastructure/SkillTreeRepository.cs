using System;
using System.Collections.Generic;
using UnityEngine;

namespace DevelopProducts.SkillTree
{
    [CreateAssetMenu(fileName = "SkillTreeRepository", menuName = "DevelopProducts/SkillTree/SkillTreeRepository")]
    public class SkillTreeRepository : ScriptableObject, ISkillTreeRepository
    {
        /// <summary>
        ///     指定されたIDのノードを取得する
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public SkillNodeEntity GetNode(SkillNodeIdVo id)
        {
            if (!_nodeDictionary.TryGetValue(id.Id, out var skillNode))
                throw new NullReferenceException($"IDが{id.Id}のスキルノードはスキルツリーに存在しません");

            return skillNode;
        }
        /// <summary>
        ///     指定されたIDのノードの親を取得する
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IReadOnlyList<SkillNodeEntity> GetParentNodes(SkillNodeIdVo id)
        {
            if (!_parentsDictionary.TryGetValue(id.Id, out var parentNodes))
                return Array.Empty<SkillNodeEntity>();

            return parentNodes;
        }
        /// <summary>
        ///     リポジトリの初期化
        /// </summary>
        public void Initialize()
        {
            //  Entityに変換
            foreach (var node in _skillNodeAsset)
            {
                node.ToDomain();
            }
            //  親子関係を構築
            foreach (var node in _skillNodeAsset)
            {
                var parents = new List<SkillNodeEntity>();
                foreach (var parent in node.Parents)
                {
                    parents.Add(parent.SkillNodeEntity);
                }
            }
            //  ノードごとの辞書作成
            foreach (var node in _skillNodeAsset)
            {
                _nodeDictionary[node.SkillNodeEntity.SkillNodeIdVO.Id] = node.SkillNodeEntity;
            }
            //  親子関係の辞書作成
            foreach (var node in _skillNodeAsset)
            {
                _parentsDictionary[node.SkillNodeEntity.SkillNodeIdVO.Id] = node.SkillNodeEntity.Parents;
            }

        }
        [SerializeField] private SkillNodeAsset[] _skillNodeAsset;
        private Dictionary<int, SkillNodeEntity> _nodeDictionary;
        private Dictionary<int, SkillNodeEntity[]> _parentsDictionary;
    }
}
