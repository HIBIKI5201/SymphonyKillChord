using KillChord.Runtime.Domain.OutGame.SkillTree;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Application.OutGame.SkillTree
{
    /// <summary>
    ///     スキルツリーの探索処理を行うクラス。
    /// </summary>
    public class SkillTreeService
    {
        public SkillTreeService(Dictionary<int, SkillNodeEntity> skillNodeEntityDict)
        {
            _skillNodeEntityDict = skillNodeEntityDict;
            _visitedNodes = new();
        }
        /// <summary>
        ///     指定されたノードまでの経路にある、全てのノードを解放するための必要ポイントを
        ///     算出し、経路にあるノードも設定する。
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="nodesOnPath"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public int TryGetTotalCost(int nodeId, HashSet<SkillNodeEntity> nodesOnPath)
        {
            if (nodesOnPath == null)
            {
                throw new ArgumentNullException(nameof(nodesOnPath), "[SkillTreeService] 引数がNULL。");
            }
            // 記録用のHashSetをクリアする
            nodesOnPath.Clear();
            _visitedNodes.Clear();

            SkillNodeEntity node = _skillNodeEntityDict[nodeId];
            // 解放済み、或いは親ノードがない場合、探索終了する
            if(node.IsUnlocked || node.Parents == null || node.Parents.Length <= 0)
            {
                return -1;
            }
            FindNodesOnPath(node, nodesOnPath);

            // 結果経路のノードの必要ポイントを合計する
            int unlockCost = 0;
            foreach (SkillNodeEntity nodeOnPath in nodesOnPath)
            {
                unlockCost += nodeOnPath.UnlockCost.Cost;
            }
            return unlockCost;
        }

        /// <summary>
        ///     指定されたノードまでの経路を取得する。
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nodesOnPath"></param>
        private void FindNodesOnPath(SkillNodeEntity node, HashSet<SkillNodeEntity> nodesOnPath)
        {
            if(node == null || _visitedNodes.Contains(node)) return;
            if (!node.IsUnlocked)
            {
                _visitedNodes.Add(node);
                nodesOnPath.Add(node);
                if (node.Parents != null && node.Parents.Length > 0)
                {
                    foreach (SkillNodeEntity parent in node.Parents)
                    {
                        FindNodesOnPath(parent, nodesOnPath);
                    }
                }
            }
        }

        private Dictionary<int, SkillNodeEntity> _skillNodeEntityDict;
        private HashSet<SkillNodeEntity> _visitedNodes;
    }
}
