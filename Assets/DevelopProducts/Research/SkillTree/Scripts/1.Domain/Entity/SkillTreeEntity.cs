using System.Collections.Generic;
using System.Linq;

namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     スキルツリー全体を管理するドメインエンティティ。
    ///     ノードの親子関係（グラフ構造）・地続き判定・動的エッジ管理を担う。
    /// </summary>
    public class SkillTreeEntity
    {
        public SkillTreeEntity(IEnumerable<SkillNodeEntity> nodes)
        {
            _nodes = nodes.ToDictionary(n => n.SkillNodeIdVO);

            _parents = _nodes.Keys.ToDictionary(id => id, _ => new List<SkillNodeIdVo>());
        }

        public SkillNodeEntity GetNode(SkillNodeIdVo id)
            => _nodes.TryGetValue(id, out var node) ? node : null;
        /// <summary>指定ノードの親ノード一覧を返す。</summary>
        public IReadOnlyList<SkillNodeEntity> GetParents(SkillNodeIdVo id)
        {
            if (!_parents.TryGetValue(id, out var parentIds))
                return System.Array.Empty<SkillNodeEntity>();

            return parentIds
                .Select(pid => _nodes[pid])
                .ToList();
        }
        /// <summary>
        ///     地続き判定。
        ///     原点から解放済みノードのみを経由して対象ノードに隣接しているか（BFS）。
        /// </summary>
        public bool IsReachable(SkillNodeIdVo targetId)
        {
            var target = GetNode(targetId);
            if (target == null || target.IsUnlocked) return false;

            // 対象ノードの親のうち1つでも解放済みであれば地続き
            return GetParents(targetId).Any(p => p.IsUnlocked);
        }
        /// <summary>原点以外のノードを全てロックする。</summary>
        public void ResetAll()
        {
            foreach (var node in _nodes.Values)
                node.Lock(); // IsRoot のノードは Lock() 内でスキップされる
        }
        private readonly Dictionary<SkillNodeIdVo, SkillNodeEntity> _nodes;
        private readonly Dictionary<SkillNodeIdVo, List<SkillNodeIdVo>> _parents;
    }
}