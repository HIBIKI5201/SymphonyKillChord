using System.Collections.Generic;

namespace KillChord.Runtime.Domain.OutGame.StageSelect
{
    /// <summary>
    ///     ステージツリーを表すクラス。
    ///     ノード間の接続関係と、解放可否のドメインルール判定を管理します。
    /// </summary>
    public sealed class StageTree
    {
        /// <summary>
        ///     StageTree を初期化します。
        /// </summary>
        /// <param name="nodes"> ツリー内の全ノード。</param>
        /// <param name="connections"> ノード間の接続情報。</param>
        public StageTree(
            IReadOnlyList<StageNode> nodes,
            IReadOnlyList<StageNodeConnection> connections)
        {
            _nodes = new Dictionary<StageId, StageNode>(nodes.Count);
            for (var i = 0; i < nodes.Count; i++)
            {
                _nodes[nodes[i].Id] = nodes[i];
            }

            _connections = connections;
        }

        /// <summary> ツリー内の全ノード。 </summary>
        public IReadOnlyCollection<StageNode> Nodes => _nodes.Values;

        /// <summary>
        ///     指定IDのノードを取得します。
        /// </summary>
        /// <param name="stageId"> 取得するステージID。</param>
        /// <param name="node"> 取得したノード。見つからない場合は null。</param>
        /// <returns> 見つかった場合は true。</returns>
        public bool TryGetNode(StageId stageId, out StageNode node)
            => _nodes.TryGetValue(stageId, out node);

        /// <summary>
        ///     指定ノードの後続ノードIDを取得します。
        /// </summary>
        /// <param name="stageId"> 起点となるステージID。</param>
        /// <returns> 後続ノードIDのリスト。</returns>
        public IReadOnlyList<StageId> GetNextIds(StageId stageId)
        {
            var result = new List<StageId>();

            for (var i = 0; i < _connections.Count; i++)
            {
                if (_connections[i].FromStageId.Equals(stageId))
                {
                    result.Add(_connections[i].ToStageId);
                }
            }

            return result;
        }

        /// <summary>
        ///     指定ノードが解放可能かどうかを判定します。
        ///     解放条件：全ての前提ノードがクリア済みであること。
        /// </summary>
        /// <param name="targetId"> 判定するステージID。</param>
        /// <returns> 解放可能な場合は true。</returns>
        public bool CanUnlock(StageId targetId)
        {
            // targetId を接続先に持つ全ての前提ノードがクリア済みか確認する
            for (var i = 0; i < _connections.Count; i++)
            {
                if (!_connections[i].ToStageId.Equals(targetId)) { continue; }

                var prereqId = _connections[i].FromStageId;
                if (!_nodes.TryGetValue(prereqId, out var prereqNode)) { return false; }

                if (prereqNode.Status != StageStatus.Cleared) { return false; }
            }

            return true;
        }

        private readonly Dictionary<StageId, StageNode> _nodes;
        private readonly IReadOnlyList<StageNodeConnection> _connections;
    }
}
