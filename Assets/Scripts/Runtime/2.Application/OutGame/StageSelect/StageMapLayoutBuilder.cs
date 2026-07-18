using KillChord.Runtime.Domain.OutGame.StageSelect;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Application.OutGame.StageSelect
{
    /// <summary>
    ///     ステージ接続を左から右へ並べるグリッド配置へ変換するクラス。
    /// </summary>
    public sealed class StageMapLayoutBuilder
    {
        /// <summary>
        ///     ステージツリーからノード配置を構築する。
        /// </summary>
        /// <param name="stageTree"> 配置対象のステージツリー。</param>
        /// <param name="rootCount"> 入力接続を持たない起点ノード数。</param>
        /// <returns> ステージID別のグリッド位置。</returns>
        public IReadOnlyDictionary<StageId, StageMapNodePosition> Build(
            StageTree stageTree,
            out int rootCount)
        {
            if (stageTree == null)
            {
                throw new ArgumentNullException(nameof(stageTree));
            }

            IReadOnlyList<StageNode> nodes = stageTree.Nodes;
            Dictionary<StageId, int> remainingIncomingCounts = new(nodes.Count);
            Dictionary<StageId, int> columns = new(nodes.Count);
            Queue<StageId> processingQueue = new();

            for (int i = 0; i < nodes.Count; i++)
            {
                StageId stageId = nodes[i].Id;
                int incomingCount = stageTree.GetPreviousIds(stageId).Count;
                remainingIncomingCounts.Add(stageId, incomingCount);
                columns.Add(stageId, 0);
                if (incomingCount == 0)
                {
                    processingQueue.Enqueue(stageId);
                }
            }

            rootCount = processingQueue.Count;
            int processedCount = AssignColumns(
                stageTree,
                processingQueue,
                remainingIncomingCounts,
                columns);
            if (processedCount != nodes.Count)
            {
                throw new InvalidOperationException(
                    "ステージ接続に循環があるため、作戦画面の自動配置を構築できません。");
            }

            return AssignRows(nodes, columns);
        }

        /// <summary>
        ///     トポロジカル順に列番号を割り当てる。
        /// </summary>
        /// <param name="stageTree"> 配置対象のステージツリー。</param>
        /// <param name="processingQueue"> 処理待ちステージID。</param>
        /// <param name="remainingIncomingCounts"> 未処理の入力接続数。</param>
        /// <param name="columns"> ステージID別の列番号。</param>
        /// <returns> 処理したノード数。</returns>
        private static int AssignColumns(
            StageTree stageTree,
            Queue<StageId> processingQueue,
            Dictionary<StageId, int> remainingIncomingCounts,
            Dictionary<StageId, int> columns)
        {
            int processedCount = 0;
            while (processingQueue.Count > 0)
            {
                StageId currentStageId = processingQueue.Dequeue();
                processedCount++;
                IReadOnlyList<StageId> nextStageIds = stageTree.GetNextIds(currentStageId);
                for (int i = 0; i < nextStageIds.Count; i++)
                {
                    StageId nextStageId = nextStageIds[i];
                    columns[nextStageId] = Math.Max(
                        columns[nextStageId],
                        columns[currentStageId] + 1);
                    remainingIncomingCounts[nextStageId]--;
                    if (remainingIncomingCounts[nextStageId] == 0)
                    {
                        processingQueue.Enqueue(nextStageId);
                    }
                }
            }

            return processedCount;
        }

        /// <summary>
        ///     同じ列のノードへ入力順に行番号を割り当てる。
        /// </summary>
        /// <param name="nodes"> 入力順を維持したノード一覧。</param>
        /// <param name="columns"> ステージID別の列番号。</param>
        /// <returns> ステージID別のグリッド位置。</returns>
        private static IReadOnlyDictionary<StageId, StageMapNodePosition> AssignRows(
            IReadOnlyList<StageNode> nodes,
            Dictionary<StageId, int> columns)
        {
            Dictionary<int, int> nextRows = new();
            Dictionary<StageId, StageMapNodePosition> positions = new(nodes.Count);
            for (int i = 0; i < nodes.Count; i++)
            {
                StageId stageId = nodes[i].Id;
                int column = columns[stageId];
                nextRows.TryGetValue(column, out int row);
                positions.Add(stageId, new StageMapNodePosition(column, row));
                nextRows[column] = row + 1;
            }

            return positions;
        }
    }
}
