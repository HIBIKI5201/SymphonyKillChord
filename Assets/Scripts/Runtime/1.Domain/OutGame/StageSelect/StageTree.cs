using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Domain.OutGame.StageSelect
{
    /// <summary>
    ///     ステージツリーを表す集約。
    ///     ノード間の接続関係と解放可否を管理する。
    /// </summary>
    public sealed class StageTree
    {
        /// <summary>
        ///     ステージツリーを初期化する。
        /// </summary>
        /// <param name="nodes"> ツリー内の全ノード。</param>
        /// <param name="connections"> ノード間の接続情報。</param>
        public StageTree(
            IReadOnlyList<StageNode> nodes,
            IReadOnlyList<StageNodeConnection> connections)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            if (connections == null)
            {
                throw new ArgumentNullException(nameof(connections));
            }

            _nodes = BuildNodeMap(nodes, out _orderedNodes, out _tutorialNode);
            _connections = CopyConnections(connections);
            BuildConnectionIndexes(
                _connections,
                out _outgoingStageIds,
                out _incomingStageIds,
                out _autoAdvanceTargetIds);
        }

        /// <summary> ツリー内の全ノード。 </summary>
        public IReadOnlyList<StageNode> Nodes => _orderedNodes;

        /// <summary> ツリー内の全接続。 </summary>
        public IReadOnlyList<StageNodeConnection> Connections => _connections;

        /// <summary>
        ///     指定IDのノードを取得する。
        /// </summary>
        /// <param name="stageId"> 取得するステージID。</param>
        /// <param name="node"> 取得したノード。見つからない場合はnull。</param>
        /// <returns> 見つかった場合はtrue。</returns>
        public bool TryGetNode(StageId stageId, out StageNode node)
        {
            return _nodes.TryGetValue(stageId, out node);
        }

        /// <summary>
        ///     指定IDのステージ定義を取得する。
        /// </summary>
        /// <param name="stageId"> 取得するステージID。</param>
        /// <param name="stageDefinition"> 取得したステージ定義。</param>
        /// <returns> 見つかった場合はtrue。</returns>
        public bool TryGetDefinition(StageId stageId, out StageDefinition stageDefinition)
        {
            if (_nodes.TryGetValue(stageId, out StageNode node)
                && node?.Definition != null)
            {
                stageDefinition = node.Definition;
                return true;
            }

            stageDefinition = null;
            return false;
        }

        /// <summary>
        ///     チュートリアルとして定義されたステージを取得する。
        /// </summary>
        /// <param name="node"> チュートリアルステージ。</param>
        /// <returns> チュートリアルステージが存在する場合はtrue。</returns>
        public bool TryGetTutorialNode(out StageNode node)
        {
            node = _tutorialNode;
            return node != null;
        }

        /// <summary>
        ///     指定ノードの後続ノードIDを取得する。
        /// </summary>
        /// <param name="stageId"> 起点となるステージID。</param>
        /// <returns> 後続ノードIDの一覧。</returns>
        public IReadOnlyList<StageId> GetNextIds(StageId stageId)
        {
            return _outgoingStageIds.TryGetValue(stageId, out StageId[] stageIds)
                ? stageIds
                : Array.Empty<StageId>();
        }

        /// <summary>
        ///     指定ノードの前提ノードIDを取得する。
        /// </summary>
        /// <param name="stageId"> 接続先となるステージID。</param>
        /// <returns> 前提ノードIDの一覧。</returns>
        public IReadOnlyList<StageId> GetPreviousIds(StageId stageId)
        {
            return _incomingStageIds.TryGetValue(stageId, out StageId[] stageIds)
                ? stageIds
                : Array.Empty<StageId>();
        }

        /// <summary>
        ///     指定ノードの自動遷移先を取得する。
        /// </summary>
        /// <param name="stageId"> 接続元のステージID。</param>
        /// <param name="stageDefinition"> 自動遷移先のステージ定義。</param>
        /// <returns> 自動遷移先が存在する場合はtrue。</returns>
        public bool TryGetAutoAdvanceTarget(
            StageId stageId,
            out StageDefinition stageDefinition)
        {
            if (_autoAdvanceTargetIds.TryGetValue(stageId, out StageId targetStageId))
            {
                return TryGetDefinition(targetStageId, out stageDefinition);
            }

            stageDefinition = null;
            return false;
        }

        /// <summary>
        ///     指定ノードが解放可能かどうかを判定する。
        ///     すべての前提ノードがクリア済みの場合に解放できる。
        /// </summary>
        /// <param name="targetId"> 判定するステージID。</param>
        /// <returns> 解放可能な場合はtrue。</returns>
        public bool CanUnlock(StageId targetId)
        {
            if (!_incomingStageIds.TryGetValue(targetId, out StageId[] prerequisiteIds))
            {
                return true;
            }

            for (int i = 0; i < prerequisiteIds.Length; i++)
            {
                if (!_nodes.TryGetValue(prerequisiteIds[i], out StageNode prerequisiteNode)
                    || prerequisiteNode.Status != StageStatus.Cleared)
                {
                    return false;
                }
            }

            return true;
        }

        private readonly Dictionary<StageId, StageNode> _nodes;
        private readonly StageNode[] _orderedNodes;
        private readonly StageNodeConnection[] _connections;
        private readonly Dictionary<StageId, StageId[]> _outgoingStageIds;
        private readonly Dictionary<StageId, StageId[]> _incomingStageIds;
        private readonly Dictionary<StageId, StageId> _autoAdvanceTargetIds;
        private readonly StageNode _tutorialNode;

        /// <summary>
        ///     ノード一覧をID検索用の辞書へ変換する。
        /// </summary>
        /// <param name="nodes"> 変換するノード一覧。</param>
        /// <param name="orderedNodes"> 入力順を維持したノード配列。</param>
        /// <param name="tutorialNode"> チュートリアルノード。</param>
        /// <returns> ノード辞書。</returns>
        private static Dictionary<StageId, StageNode> BuildNodeMap(
            IReadOnlyList<StageNode> nodes,
            out StageNode[] orderedNodes,
            out StageNode tutorialNode)
        {
            Dictionary<StageId, StageNode> nodeMap = new(nodes.Count);
            List<StageNode> orderedNodeBuilder = new(nodes.Count);
            tutorialNode = null;

            for (int i = 0; i < nodes.Count; i++)
            {
                StageNode node = nodes[i];
                if (node == null)
                {
                    continue;
                }

                if (!nodeMap.TryAdd(node.Id, node))
                {
                    throw new InvalidOperationException(
                        $"StageIdが重複しています。StageId: {node.Id.Value}");
                }

                orderedNodeBuilder.Add(node);

                if (!node.Definition.IsTutorial)
                {
                    continue;
                }

                if (tutorialNode != null)
                {
                    throw new InvalidOperationException("チュートリアルステージが複数定義されています。");
                }

                tutorialNode = node;
            }

            orderedNodes = orderedNodeBuilder.ToArray();
            return nodeMap;
        }

        /// <summary>
        ///     接続一覧を不変な配列へコピーする。
        /// </summary>
        /// <param name="connections"> コピー元の接続一覧。</param>
        /// <returns> コピーした接続配列。</returns>
        private static StageNodeConnection[] CopyConnections(
            IReadOnlyList<StageNodeConnection> connections)
        {
            StageNodeConnection[] result = new StageNodeConnection[connections.Count];
            for (int i = 0; i < connections.Count; i++)
            {
                result[i] = connections[i];
            }

            return result;
        }

        /// <summary>
        ///     接続情報を実行時検索用の辞書へ変換する。
        /// </summary>
        /// <param name="connections"> 変換する接続情報。</param>
        /// <param name="outgoingStageIds"> 接続元別の後続ID辞書。</param>
        /// <param name="incomingStageIds"> 接続先別の前提ID辞書。</param>
        /// <param name="autoAdvanceTargetIds"> 接続元別の自動遷移先ID辞書。</param>
        private void BuildConnectionIndexes(
            IReadOnlyList<StageNodeConnection> connections,
            out Dictionary<StageId, StageId[]> outgoingStageIds,
            out Dictionary<StageId, StageId[]> incomingStageIds,
            out Dictionary<StageId, StageId> autoAdvanceTargetIds)
        {
            Dictionary<StageId, List<StageId>> outgoingBuilder = new();
            Dictionary<StageId, List<StageId>> incomingBuilder = new();
            autoAdvanceTargetIds = new Dictionary<StageId, StageId>();
            HashSet<ConnectionKey> connectionKeys = new();

            for (int i = 0; i < connections.Count; i++)
            {
                StageNodeConnection connection = connections[i];
                ValidateConnection(connection, connectionKeys, autoAdvanceTargetIds);
                AddIndexValue(outgoingBuilder, connection.FromStageId, connection.ToStageId);
                AddIndexValue(incomingBuilder, connection.ToStageId, connection.FromStageId);
            }

            outgoingStageIds = FreezeIndexes(outgoingBuilder);
            incomingStageIds = FreezeIndexes(incomingBuilder);
            ValidateAcyclicGraph(outgoingStageIds, incomingStageIds);
        }

        /// <summary>
        ///     接続グラフが循環を持たないことを検証する。
        /// </summary>
        /// <param name="outgoingStageIds"> 接続元別の後続ID辞書。</param>
        /// <param name="incomingStageIds"> 接続先別の前提ID辞書。</param>
        private void ValidateAcyclicGraph(
            Dictionary<StageId, StageId[]> outgoingStageIds,
            Dictionary<StageId, StageId[]> incomingStageIds)
        {
            Dictionary<StageId, int> remainingIncomingCounts = new(_nodes.Count);
            Queue<StageId> processingQueue = new();
            foreach (StageId stageId in _nodes.Keys)
            {
                int incomingCount = incomingStageIds.TryGetValue(
                    stageId,
                    out StageId[] incomingIds)
                    ? incomingIds.Length
                    : 0;
                remainingIncomingCounts.Add(stageId, incomingCount);
                if (incomingCount == 0)
                {
                    processingQueue.Enqueue(stageId);
                }
            }

            int processedCount = 0;
            while (processingQueue.Count > 0)
            {
                StageId currentStageId = processingQueue.Dequeue();
                processedCount++;
                if (!outgoingStageIds.TryGetValue(
                        currentStageId,
                        out StageId[] nextStageIds))
                {
                    continue;
                }

                for (int i = 0; i < nextStageIds.Length; i++)
                {
                    StageId nextStageId = nextStageIds[i];
                    remainingIncomingCounts[nextStageId]--;
                    if (remainingIncomingCounts[nextStageId] == 0)
                    {
                        processingQueue.Enqueue(nextStageId);
                    }
                }
            }

            if (processedCount != _nodes.Count)
            {
                throw new InvalidOperationException(
                    "ステージ接続グラフに循環があります。StageTreeはDAGである必要があります。");
            }
        }

        /// <summary>
        ///     接続情報を検証する。
        /// </summary>
        /// <param name="connection"> 検証する接続。</param>
        /// <param name="connectionKeys"> 登録済み接続キー。</param>
        /// <param name="autoAdvanceTargetIds"> 自動遷移先辞書。</param>
        private void ValidateConnection(
            StageNodeConnection connection,
            HashSet<ConnectionKey> connectionKeys,
            Dictionary<StageId, StageId> autoAdvanceTargetIds)
        {
            if (!_nodes.ContainsKey(connection.FromStageId)
                || !_nodes.ContainsKey(connection.ToStageId))
            {
                throw new InvalidOperationException("StageTreeに存在しないステージへの接続があります。");
            }

            if (connection.FromStageId.Equals(connection.ToStageId))
            {
                throw new InvalidOperationException(
                    $"自己接続は設定できません。StageId: {connection.FromStageId.Value}");
            }

            ConnectionKey connectionKey = new(connection.FromStageId, connection.ToStageId);
            if (!connectionKeys.Add(connectionKey))
            {
                throw new InvalidOperationException(
                    $"接続が重複しています。From: {connection.FromStageId.Value}, To: {connection.ToStageId.Value}");
            }

            if (connection.AdvanceMode != StageAdvanceMode.AutoAdvance)
            {
                return;
            }

            if (!autoAdvanceTargetIds.TryAdd(connection.FromStageId, connection.ToStageId))
            {
                throw new InvalidOperationException(
                    $"同じ接続元から複数の自動遷移は設定できません。From: {connection.FromStageId.Value}");
            }
        }

        /// <summary>
        ///     索引構築用のリストへ値を追加する。
        /// </summary>
        /// <param name="indexes"> 索引辞書。</param>
        /// <param name="key"> 索引キー。</param>
        /// <param name="value"> 追加する値。</param>
        private static void AddIndexValue(
            Dictionary<StageId, List<StageId>> indexes,
            StageId key,
            StageId value)
        {
            if (!indexes.TryGetValue(key, out List<StageId> values))
            {
                values = new List<StageId>();
                indexes.Add(key, values);
            }

            values.Add(value);
        }

        /// <summary>
        ///     構築用の可変索引を配列索引へ変換する。
        /// </summary>
        /// <param name="source"> 変換元の索引。</param>
        /// <returns> 配列へ変換した索引。</returns>
        private static Dictionary<StageId, StageId[]> FreezeIndexes(
            Dictionary<StageId, List<StageId>> source)
        {
            Dictionary<StageId, StageId[]> result = new(source.Count);
            foreach (KeyValuePair<StageId, List<StageId>> pair in source)
            {
                result.Add(pair.Key, pair.Value.ToArray());
            }

            return result;
        }

        /// <summary>
        ///     接続元と接続先の組を表す内部値型。
        /// </summary>
        private readonly struct ConnectionKey : IEquatable<ConnectionKey>
        {
            /// <summary>
            ///     接続キーを初期化する。
            /// </summary>
            /// <param name="fromStageId"> 接続元ID。</param>
            /// <param name="toStageId"> 接続先ID。</param>
            public ConnectionKey(StageId fromStageId, StageId toStageId)
            {
                _fromStageId = fromStageId;
                _toStageId = toStageId;
            }

            /// <summary>
            ///     接続キーを比較する。
            /// </summary>
            /// <param name="other"> 比較対象。</param>
            /// <returns> 等しい場合はtrue。</returns>
            public bool Equals(ConnectionKey other)
            {
                return _fromStageId.Equals(other._fromStageId)
                    && _toStageId.Equals(other._toStageId);
            }

            /// <summary>
            ///     接続キーを比較する。
            /// </summary>
            /// <param name="obj"> 比較対象。</param>
            /// <returns> 等しい場合はtrue。</returns>
            public override bool Equals(object obj)
            {
                return obj is ConnectionKey other && Equals(other);
            }

            /// <summary>
            ///     ハッシュコードを取得する。
            /// </summary>
            /// <returns> ハッシュコード。</returns>
            public override int GetHashCode()
            {
                return HashCode.Combine(_fromStageId, _toStageId);
            }

            private readonly StageId _fromStageId;
            private readonly StageId _toStageId;
        }
    }
}
