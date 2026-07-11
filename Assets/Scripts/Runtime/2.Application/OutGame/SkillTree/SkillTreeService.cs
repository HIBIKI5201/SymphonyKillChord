using KillChord.Runtime.Domain.OutGame.SkillTree;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.Utility.OutGame.Savedata;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.Application.OutGame.SkillTree
{
    /// <summary>
    ///     スキルツリーの探索処理を行うクラス。
    /// </summary>
    public class SkillTreeService
    {
        /// <summary>
        ///     スキルツリー処理で使用する依存関係を初期化する。
        /// </summary>
        /// <param name="skillNodeEntityDict"> ノード一覧です。 </param>
        /// <param name="savedataSystem"> セーブデータシステムです。 </param>
        public SkillTreeService(
            Dictionary<int, SkillNodeEntity> skillNodeEntityDict,
            SavedataSystem savedataSystem)
        {
            _skillNodeEntityDict = skillNodeEntityDict ?? throw new ArgumentNullException(nameof(skillNodeEntityDict));
            _visitedNodes = new();
            _savedataSystem = savedataSystem ?? throw new ArgumentNullException(nameof(savedataSystem));
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

            if (!_skillNodeEntityDict.TryGetValue(nodeId, out SkillNodeEntity node))
            {
                Debug.LogWarning($"[SkillTreeService] ノードID {nodeId} が見つかりません。");
                return -1;
            }
            // 解放済み、或いは親ノードがない場合、探索終了する
            if (node.IsUnlocked || node.Parents == null || node.Parents.Length <= 0)
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
        ///     セーブデータのスキル解放情報をロードする。
        /// </summary>
        /// <returns></returns>
        public async ValueTask<SkillUnlockData> LoadSkillUnlockData()
        {
            SaveData saveData = await _savedataSystem.LoadAsync<SaveData>();
            return saveData.SkillUnlock;
        }

        /// <summary>
        ///     スキル解放情報をセーブする。
        /// </summary>
        /// <param name="unlockedNodes"></param>
        /// <param name="unlockedSkillIds"></param>
        /// <param name="currentPoints"></param>
        public async Task SaveSkillUnlockData(List<int> unlockedNodes, List<int> unlockedSkillIds, int currentPoints)
        {
            SaveData saveData = await _savedataSystem.LoadAsync<SaveData>();
            saveData.SkillUnlock.SetUnlockedSkillNodeIds(unlockedNodes.ToArray());
            saveData.SkillUnlock.SetUnlockedSkillIds(unlockedSkillIds.ToArray());
            saveData.SkillUnlock.SetResearchPoint(currentPoints);
            await _savedataSystem.SaveAsync(saveData);
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

        private readonly Dictionary<int, SkillNodeEntity> _skillNodeEntityDict;
        private readonly HashSet<SkillNodeEntity> _visitedNodes;
        private readonly SavedataSystem _savedataSystem;
    }
}
