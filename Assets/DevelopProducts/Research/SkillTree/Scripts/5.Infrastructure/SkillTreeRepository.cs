using System;
using System.Collections.Generic;
using UnityEngine;

namespace DevelopProducts.SkillTree
{
    [CreateAssetMenu(fileName = "SkillTreeRepository", menuName = "DevelopProducts/SkillTree/SkillTreeRepository")]
    public class SkillTreeRepository : ScriptableObject, ISkillTreeRepository
    {
        public IReadOnlyCollection<SkillNodeEntity> SkillNodeEntities => _nodeDictionary.Values;
        /// <summary>
        ///     指定されたIDのノードを取得する
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
        ///     指定されたIDのノードの親を取得する
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
        ///     リポジトリの初期化
        /// </summary>
        public void Initialize()
        {
            _nodeDictionary = new Dictionary<int, SkillNodeEntity>();
            _parentsDictionary = new Dictionary<int, SkillNodeEntity[]>();
            //  Entityに変換
            foreach (var node in _skillNodeAsset)
            {
                node.ToDomain();
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
