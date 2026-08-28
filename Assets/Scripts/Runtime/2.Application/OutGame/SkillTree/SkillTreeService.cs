using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.OutGame.SkillTree;
using KillChord.Runtime.Domain.Persistent.Savedata;
using SymphonyFrameWork.System.SaveSystem;
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
        public SkillTreeService(Dictionary<SkillNodeId, SkillNodeEntity> skillNodeEntityDict)
        {
            _skillNodeEntityDict = skillNodeEntityDict ?? throw new ArgumentNullException(nameof(skillNodeEntityDict));
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
        public int TryGetTotalCost(SkillNodeId nodeId, HashSet<SkillNodeEntity> nodesOnPath)
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
            // 解放済みの場合は支払うコストがないため探索終了する。
            // 親ノードを持たない起点ノードは、自身のコストのみで解放できるため探索を続行する。
            if (node.IsUnlocked)
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
        ///     指定候補のうち、スタート地点から最も近い未解放ノードIDを取得する。
        ///     指定候補が全て解放済みの場合は、最も遠い解放済みノードIDを取得する。
        /// </summary>
        /// <param name="candidateNodeIds"> 初期フォーカス対象になり得るノードID。 </param>
        /// <param name="nodeIds"> 初期フォーカス対象のノードID。 </param>
        /// <returns> 親参照の距離を算出できた場合はtrue。 </returns>
        public bool TryGetNearestLockedNodeIdsFromStart(
            IReadOnlyCollection<SkillNodeId> candidateNodeIds,
            out IReadOnlyList<SkillNodeId> nodeIds)
        {
            if (candidateNodeIds == null)
            {
                throw new ArgumentNullException(nameof(candidateNodeIds));
            }

            nodeIds = Array.Empty<SkillNodeId>();
            HashSet<SkillNodeEntity> candidateNodes = new HashSet<SkillNodeEntity>();
            foreach (SkillNodeId candidateNodeId in candidateNodeIds)
            {
                if (_skillNodeEntityDict.TryGetValue(
                        candidateNodeId,
                        out SkillNodeEntity candidateNode))
                {
                    candidateNodes.Add(candidateNode);
                }
            }

            if (candidateNodes.Count == 0)
            {
                return true;
            }

            Dictionary<SkillNodeEntity, int> distanceByNode =
                new Dictionary<SkillNodeEntity, int>(candidateNodes.Count);
            HashSet<SkillNodeEntity> nodesBeingCalculated = new HashSet<SkillNodeEntity>();
            foreach (SkillNodeEntity node in candidateNodes)
            {
                if (!TryGetDistanceFromStart(
                        node,
                        distanceByNode))
                {
                    return false;
                }
            }

            List<SkillNodeId> targetNodeIds = new List<SkillNodeId>();
            int targetDistance = int.MaxValue;
            foreach (SkillNodeEntity node in candidateNodes)
            {
                if (node.IsUnlocked)
                {
                    continue;
                }

                int distance = distanceByNode[node];
                if (distance < targetDistance)
                {
                    targetDistance = distance;
                    targetNodeIds.Clear();
                }

                if (distance == targetDistance)
                {
                    targetNodeIds.Add(node.SkillNodeIdVO);
                }
            }

            if (targetNodeIds.Count == 0)
            {
                targetDistance = int.MinValue;
                foreach (SkillNodeEntity node in candidateNodes)
                {
                    int distance = distanceByNode[node];
                    if (distance > targetDistance)
                    {
                        targetDistance = distance;
                        targetNodeIds.Clear();
                    }

                    if (distance == targetDistance)
                    {
                        targetNodeIds.Add(node.SkillNodeIdVO);
                    }
                }
            }

            targetNodeIds.Sort((left, right) => left.Id.CompareTo(right.Id));
            nodeIds = targetNodeIds;
            return true;
        }

        /// <summary>
        ///     セーブデータのスキル解放情報をロードする。
        /// </summary>
        /// <returns></returns>
        public async ValueTask<SkillUnlockData> LoadSkillUnlockData()
        {
            SaveData saveData = SaveStore.IsLoaded<SaveData>()
                ? SaveStore.Get<SaveData>()
                : await SaveStore.LoadAsync<SaveData>();
            return saveData.SkillUnlock;
        }

        /// <summary>
        ///     スキル解放情報をセーブする。
        /// </summary>
        /// <param name="unlockedNodes"></param>
        /// <param name="unlockedSkillIds"></param>
        /// <param name="currentPoints"></param>
        public async Task SaveSkillUnlockData(
            IReadOnlyList<SkillNodeId> unlockedNodes,
            IReadOnlyList<SkillId> unlockedSkillIds,
            int currentPoints)
        {
            SaveData saveData = SaveStore.IsLoaded<SaveData>()
                ? SaveStore.Get<SaveData>()
                : await SaveStore.LoadAsync<SaveData>();
            int[] unlockedNodeValues = new int[unlockedNodes.Count];
            for (int i = 0; i < unlockedNodes.Count; i++)
            {
                unlockedNodeValues[i] = unlockedNodes[i].Id;
            }

            int[] unlockedSkillValues = new int[unlockedSkillIds.Count];
            for (int i = 0; i < unlockedSkillIds.Count; i++)
            {
                unlockedSkillValues[i] = unlockedSkillIds[i].Value;
            }

            int previousPoints = saveData.SkillUnlock.ResearchPoint;
            int[] previousNodeIds = (int[])saveData.SkillUnlock.UnlockedSkillNodeIds.Clone();
            int[] previousSkillIds = (int[])saveData.SkillUnlock.UnlockedSkillIds.Clone();

            saveData.SkillUnlock.SetUnlockedSkillNodeIds(unlockedNodeValues);
            saveData.SkillUnlock.SetUnlockedSkillIds(unlockedSkillValues);
            saveData.SkillUnlock.SetResearchPoint(currentPoints);

            try
            {
                await SaveStore.SaveAsync<SaveData>();
            }
            catch
            {
                // SaveStore が返すキャッシュ参照を、保存試行前の状態へ戻す。
                saveData.SkillUnlock.SetResearchPoint(previousPoints);
                saveData.SkillUnlock.SetUnlockedSkillNodeIds(previousNodeIds);
                saveData.SkillUnlock.SetUnlockedSkillIds(previousSkillIds);
                throw;
            }
        }

        /// <summary>
        ///     現在の解放状態をリセットした場合に返却される研究ポイントを算出する。
        /// </summary>
        /// <param name="unlockedNodes"> 現在解放されているノード。 </param>
        /// <returns> 返却される研究ポイント。 </returns>
        public int CalculateResetRefundPoints(IReadOnlyCollection<SkillNodeId> unlockedNodes)
        {
            if (unlockedNodes == null)
            {
                throw new ArgumentNullException(nameof(unlockedNodes));
            }

            int refundPoints = 0;
            HashSet<SkillNodeId> processedNodeIds = new HashSet<SkillNodeId>();
            foreach (SkillNodeId nodeId in unlockedNodes)
            {
                if (processedNodeIds.Add(nodeId)
                    && _skillNodeEntityDict.TryGetValue(nodeId, out SkillNodeEntity node)
                    && !IsInitialNode(node))
                {
                    refundPoints += node.UnlockCost.Cost;
                }
            }

            return refundPoints;
        }

        /// <summary>
        ///     スキルツリーを初期状態へ戻し、関連する装備データと共に保存する。
        /// </summary>
        /// <param name="unlockedNodes"> 現在解放されているノード。 </param>
        /// <param name="unlockedSkillIds"> 現在所持しているスキル。 </param>
        /// <param name="currentPoints"> 現在の研究ポイント。 </param>
        /// <returns> 保存済みのリセット結果。 </returns>
        public async Task<SkillTreeResetResult> ResetSkillTreeAsync(
            IReadOnlyCollection<SkillNodeId> unlockedNodes,
            IReadOnlyCollection<SkillId> unlockedSkillIds,
            int currentPoints)
        {
            if (unlockedNodes == null)
            {
                throw new ArgumentNullException(nameof(unlockedNodes));
            }

            if (unlockedSkillIds == null)
            {
                throw new ArgumentNullException(nameof(unlockedSkillIds));
            }

            int refundPoints = CalculateResetRefundPoints(unlockedNodes);
            List<SkillNodeId> remainingNodes = new List<SkillNodeId>();
            HashSet<SkillId> removedSkillIds = new HashSet<SkillId>();
            HashSet<SkillNodeId> processedNodeIds = new HashSet<SkillNodeId>();
            foreach (SkillNodeId nodeId in unlockedNodes)
            {
                if (!processedNodeIds.Add(nodeId)
                    || !_skillNodeEntityDict.TryGetValue(nodeId, out SkillNodeEntity node))
                {
                    continue;
                }

                if (IsInitialNode(node))
                {
                    remainingNodes.Add(nodeId);
                    continue;
                }

                for (int i = 0; i < node.UnlockSkillIds.Length; i++)
                {
                    removedSkillIds.Add(node.UnlockSkillIds[i]);
                }
            }

            HashSet<SkillId> remainingSkillSet = new HashSet<SkillId>();
            foreach (SkillId skillId in unlockedSkillIds)
            {
                if (!removedSkillIds.Contains(skillId))
                {
                    remainingSkillSet.Add(skillId);
                }
            }

            for (int i = 0; i < remainingNodes.Count; i++)
            {
                SkillNodeEntity node = _skillNodeEntityDict[remainingNodes[i]];
                for (int skillIndex = 0; skillIndex < node.UnlockSkillIds.Length; skillIndex++)
                {
                    remainingSkillSet.Add(node.UnlockSkillIds[skillIndex]);
                }
            }

            SkillId[] remainingSkills = new SkillId[remainingSkillSet.Count];
            remainingSkillSet.CopyTo(remainingSkills);
            int resetPoints = checked(currentPoints + refundPoints);
            SaveData saveData = SaveStore.IsLoaded<SaveData>()
                ? SaveStore.Get<SaveData>()
                : await SaveStore.LoadAsync<SaveData>();
            int previousPoints = saveData.SkillUnlock.ResearchPoint;
            int[] previousNodeIds = (int[])saveData.SkillUnlock.UnlockedSkillNodeIds.Clone();
            int[] previousSkillIds = (int[])saveData.SkillUnlock.UnlockedSkillIds.Clone();
            List<int> previousEquipmentSkillIds = new List<int>(saveData.SkillBuild.EquipmentSkillIDs);
            try
            {
                saveData.SkillUnlock.SetResearchPoint(resetPoints);
                saveData.SkillUnlock.SetUnlockedSkillNodeIds(ConvertNodeIds(remainingNodes));
                saveData.SkillUnlock.SetUnlockedSkillIds(ConvertSkillIds(remainingSkills));
                saveData.SkillBuild.SetEquipmentSkillIDs(FilterEquippedSkillIds(
                    saveData.SkillBuild.EquipmentSkillIDs,
                    remainingSkillSet));
                await SaveStore.SaveAsync<SaveData>();
            }
            catch
            {
                // SaveStore が返すキャッシュ参照を、保存試行前の状態へ戻す。
                saveData.SkillUnlock.SetResearchPoint(previousPoints);
                saveData.SkillUnlock.SetUnlockedSkillNodeIds(previousNodeIds);
                saveData.SkillUnlock.SetUnlockedSkillIds(previousSkillIds);
                saveData.SkillBuild.SetEquipmentSkillIDs(previousEquipmentSkillIds);
                throw;
            }

            return new SkillTreeResetResult(
                refundPoints,
                resetPoints,
                remainingNodes.ToArray(),
                remainingSkills);
        }

        /// <summary>
        ///     指定されたノードまでの経路を取得する。
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nodesOnPath"></param>
        private void FindNodesOnPath(SkillNodeEntity node, HashSet<SkillNodeEntity> nodesOnPath)
        {
            if (node == null || _visitedNodes.Contains(node)) return;
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

        /// <summary>
        ///     親参照を辿り、スタート地点から指定ノードまでの最短距離を取得する。
        /// </summary>
        /// <param name="startNode"> 距離を取得する起点ノード。 </param>
        /// <param name="distanceByNode"> 計算済み距離のキャッシュ。複数回の呼び出し間で共有可能。 </param>
        /// <returns> 循環せず距離を算出できた場合はtrue。 </returns>
        private bool TryGetDistanceFromStart(
            SkillNodeEntity startNode,
            Dictionary<SkillNodeEntity, int> distanceByNode)
        {
            if (startNode == null)
            {
                return false;
            }

            if (distanceByNode.ContainsKey(startNode))
            {
                return true;
            }

            // フィールドを使い回すため、呼び出しの都度クリアしてから利用する。
            Stack<(SkillNodeEntity Node, int ParentIndex, int MinParentDistance)> workStack = _distanceWorkStack;
            HashSet<SkillNodeEntity> nodesOnStack = _distanceNodesOnStack;
            workStack.Clear();
            nodesOnStack.Clear();

            workStack.Push((startNode, 0, int.MaxValue));
            nodesOnStack.Add(startNode);

            while (workStack.Count > 0)
            {
                (SkillNodeEntity node, int parentIndex, int minParentDistance) = workStack.Pop();

                SkillNodeEntity[] parents = node.Parents;
                if (parents == null || parents.Length == 0)
                {
                    distanceByNode[node] = 0;
                    nodesOnStack.Remove(node);
                    continue;
                }

                bool suspended = false;
                for (int i = parentIndex; i < parents.Length; i++)
                {
                    SkillNodeEntity parent = parents[i];
                    if (parent == null)
                    {
                        workStack.Clear();
                        nodesOnStack.Clear();
                        return false;
                    }

                    if (distanceByNode.TryGetValue(parent, out int parentDistance))
                    {
                        minParentDistance = Math.Min(minParentDistance, parentDistance);
                        continue;
                    }

                    // 現在の探索経路上に既に存在する場合は循環参照。
                    if (!nodesOnStack.Add(parent))
                    {
                        workStack.Clear();
                        nodesOnStack.Clear();
                        return false;
                    }

                    // 未計算の親を先に処理するため、自身を中断状態で積み直してから親を積む。
                    workStack.Push((node, i, minParentDistance));
                    workStack.Push((parent, 0, int.MaxValue));
                    suspended = true;
                    break;
                }

                if (!suspended)
                {
                    distanceByNode[node] = minParentDistance == int.MaxValue ? 0 : minParentDistance + 1;
                    nodesOnStack.Remove(node);
                }
            }

            return true;
        }

        /// <summary>
        ///     リセット後も維持する初期ノードか判定する。
        /// </summary>
        /// <param name="node"> 判定対象のノード。 </param>
        /// <returns> 初期ノードの場合はtrue。 </returns>
        private static bool IsInitialNode(SkillNodeEntity node)
        {
            return node.UnlockCost.Cost == 0 && (node.Parents == null || node.Parents.Length == 0);
        }

        /// <summary>
        ///     ノードIDを永続化用の数値配列へ変換する。
        /// </summary>
        /// <param name="nodeIds"> ノードID一覧。 </param>
        /// <returns> 数値ID配列。 </returns>
        private static int[] ConvertNodeIds(IReadOnlyList<SkillNodeId> nodeIds)
        {
            int[] values = new int[nodeIds.Count];
            for (int i = 0; i < nodeIds.Count; i++)
            {
                values[i] = nodeIds[i].Id;
            }

            return values;
        }

        /// <summary>
        ///     スキルIDを永続化用の数値配列へ変換する。
        /// </summary>
        /// <param name="skillIds"> スキルID一覧。 </param>
        /// <returns> 数値ID配列。 </returns>
        private static int[] ConvertSkillIds(IReadOnlyList<SkillId> skillIds)
        {
            int[] values = new int[skillIds.Count];
            for (int i = 0; i < skillIds.Count; i++)
            {
                values[i] = skillIds[i].Value;
            }

            return values;
        }

        /// <summary>
        ///     リセット後も所持しているスキルだけを装備一覧へ残す。
        /// </summary>
        /// <param name="equippedSkillIds"> 現在の装備スキルID。 </param>
        /// <param name="remainingSkillIds"> リセット後の所持スキルID。 </param>
        /// <returns> リセット後の装備スキルID。 </returns>
        private static List<int> FilterEquippedSkillIds(
            IReadOnlyList<int> equippedSkillIds,
            HashSet<SkillId> remainingSkillIds)
        {
            List<int> result = new List<int>();
            if (equippedSkillIds == null)
            {
                return result;
            }

            for (int i = 0; i < equippedSkillIds.Count; i++)
            {
                int skillIdValue = equippedSkillIds[i];
                bool isOwned = skillIdValue != EMPTY_SKILL_ID
                    && remainingSkillIds.Contains(new SkillId(skillIdValue));
                result.Add(isOwned ? skillIdValue : EMPTY_SKILL_ID);
            }

            return result;
        }

        private const int EMPTY_SKILL_ID = -1;

        private readonly Dictionary<SkillNodeId, SkillNodeEntity> _skillNodeEntityDict;
        private readonly HashSet<SkillNodeEntity> _visitedNodes;

        // TryGetDistanceFromStart で使い回すための作業用フィールド。
        private readonly Stack<(SkillNodeEntity Node, int ParentIndex, int MinParentDistance)> _distanceWorkStack = new();
        private readonly HashSet<SkillNodeEntity> _distanceNodesOnStack = new();
    }
}
