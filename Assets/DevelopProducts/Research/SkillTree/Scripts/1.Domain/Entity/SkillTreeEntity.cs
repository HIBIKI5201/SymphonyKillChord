using System.Collections.Generic;
using System.Linq;

namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     スキルツリー全体を管理するドメインエンティティ。
    /// </summary>
    public class SkillTreeEntity
    {
        public SkillTreeEntity(IEnumerable<SkillNodeEntity> nodes)
        {
            _nodes = nodes.ToDictionary(n => n.SkillNodeIdVO);

            _parents = _nodes.Keys.ToDictionary(id => id, _ => new List<SkillNodeId>());
            foreach (var node in _nodes.Values)
            {
                if (node?.Parents == null)
                    continue;

                var parentIds = _parents[node.SkillNodeIdVO];
                foreach (var parent in node.Parents)
                {
                    if (parent == null)
                        continue;

                    parentIds.Add(parent.SkillNodeIdVO);
                }
            }
        }

        public SkillNodeEntity GetNode(SkillNodeId id)
            => _nodes.TryGetValue(id, out var node) ? node : null;
        /// <summary>指定ノードの親ノード一覧を返す。</summary>
        public IReadOnlyList<SkillNodeEntity> GetParents(SkillNodeId id)
        {
            if (!_parents.TryGetValue(id, out var parentIds))
                return System.Array.Empty<SkillNodeEntity>();

            return parentIds
                .Select(pid => _nodes[pid])
                .ToList();
        }
        /// <summary>原点以外のノードを全てロックする。</summary>
        public void ResetAll()
        {
            foreach (var node in _nodes.Values)
                node.Lock(); // IsRoot のノードは Lock() 内でスキップされる
        }
        private readonly Dictionary<SkillNodeId, SkillNodeEntity> _nodes;
        private readonly Dictionary<SkillNodeId, List<SkillNodeId>> _parents;
    }
}