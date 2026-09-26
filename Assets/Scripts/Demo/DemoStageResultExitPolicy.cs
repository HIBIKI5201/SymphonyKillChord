using KillChord.Runtime.Adaptor.InGame.Result;
using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.Domain.Persistent.Savedata;
using SymphonyFrameWork.System.SaveSystem;
using System;
using System.Collections.Generic;

namespace KillChord.Demo
{
    /// <summary>
    ///     全体制限時間切れ、またはステージツリー上の最終バトルクリア後のリザルトを
    ///     体験版終了へ接続します。
    /// </summary>
    public sealed class DemoStageResultExitPolicy : IStageResultExitPolicy
    {
        /// <summary>
        ///     ポリシーを生成します。
        /// </summary>
        /// <param name="sessionState"> 継続中の体験版セッション状態です。 </param>
        /// <param name="config"> 体験版設定です。 </param>
        public DemoStageResultExitPolicy(DemoSessionState sessionState, DemoExperienceConfig config)
        {
            _sessionState = sessionState;
            _config = config;
        }

        /// <summary>
        ///     ステージツリーの解放順から最終バトルステージを設定します。
        /// </summary>
        /// <param name="stageTree"> 体験版で使用するステージツリーです。 </param>
        /// <returns> 最終バトルステージを設定できた場合はtrueです。 </returns>
        public bool TryConfigureFinalStage(StageTree stageTree)
        {
            if (!TryResolveLatestBattleNode(
                    stageTree,
                    false,
                    out StageNode finalBattleNode))
            {
                return false;
            }

            _finalStageId = finalBattleNode.Id;
            _hasFinalStage = true;
            return true;
        }

        /// <summary>
        ///     選択可能なバトルステージのうち、解放順が最も新しいステージを取得します。
        /// </summary>
        /// <param name="stageTree"> 体験版で使用するステージツリーです。 </param>
        /// <param name="stageDefinition"> 取得したバトルステージ定義です。 </param>
        /// <returns> 対象を取得できた場合はtrueです。 </returns>
        public static bool TryGetLatestAvailableBattleStage(
            StageTree stageTree,
            out BattleStageDefinition stageDefinition)
        {
            if (TryResolveLatestBattleNode(
                    stageTree,
                    true,
                    out StageNode latestBattleNode))
            {
                stageDefinition = (BattleStageDefinition)latestBattleNode.Definition;
                return true;
            }

            stageDefinition = null;
            return false;
        }

        /// <summary>
        ///     条件に合うバトルステージのうち、解放までの距離が最も長いノードを取得します。
        /// </summary>
        private static bool TryResolveLatestBattleNode(
            StageTree stageTree,
            bool availableOnly,
            out StageNode latestBattleNode)
        {
            latestBattleNode = null;
            if (stageTree == null)
            {
                return false;
            }

            Dictionary<StageId, int> unlockDepths = new();
            HashSet<StageId> visitingStageIds = new();
            int latestUnlockDepth = -1;

            IReadOnlyList<StageNode> nodes = stageTree.Nodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                StageNode node = nodes[i];
                if (node?.Definition is not BattleStageDefinition
                    || (availableOnly && node.Status == StageStatus.Locked))
                {
                    continue;
                }

                if (!TryResolveUnlockDepth(
                        stageTree,
                        node.Id,
                        unlockDepths,
                        visitingStageIds,
                        out int unlockDepth))
                {
                    continue;
                }

                if (unlockDepth <= latestUnlockDepth)
                {
                    continue;
                }

                latestBattleNode = node;
                latestUnlockDepth = unlockDepth;
            }

            return latestBattleNode != null;
        }

        /// <inheritdoc />
        public bool TryGetDestinationScene(
            StageResultExitAction action,
            SelectedBattleStageState selectedBattleStageState,
            out string destinationSceneName)
        {
            destinationSceneName = string.Empty;
            if (selectedBattleStageState == null
                || !selectedBattleStageState.HasSelectedBattleStage
                || string.IsNullOrWhiteSpace(_config.EndSceneName))
            {
                return false;
            }

            int currentStageId = selectedBattleStageState.CurrentStageDefinition.StageId.Value;
            bool isFinalStageCleared = _hasFinalStage
                && currentStageId == _finalStageId.Value
                && SaveStore.IsLoaded<SaveData>()
                && SaveStore.Get<SaveData>().StageProgress.IsStageCleared(currentStageId);
            if (!_sessionState.IsOverallTimeExpired && !isFinalStageCleared)
            {
                return false;
            }

            destinationSceneName = _config.EndSceneName;
            return true;
        }

        /// <summary>
        ///     前提接続をたどり、対象ステージが解放されるまでの最長距離を取得します。
        /// </summary>
        private static bool TryResolveUnlockDepth(
            StageTree stageTree,
            StageId stageId,
            Dictionary<StageId, int> cachedDepths,
            HashSet<StageId> visitingStageIds,
            out int unlockDepth)
        {
            if (cachedDepths.TryGetValue(stageId, out unlockDepth))
            {
                return true;
            }

            if (!visitingStageIds.Add(stageId))
            {
                unlockDepth = 0;
                return false;
            }

            IReadOnlyList<StageId> previousIds = stageTree.GetPreviousIds(stageId);
            unlockDepth = 0;
            for (int i = 0; i < previousIds.Count; i++)
            {
                if (!TryResolveUnlockDepth(
                        stageTree,
                        previousIds[i],
                        cachedDepths,
                        visitingStageIds,
                        out int previousUnlockDepth))
                {
                    visitingStageIds.Remove(stageId);
                    unlockDepth = 0;
                    return false;
                }

                unlockDepth = Math.Max(unlockDepth, previousUnlockDepth + 1);
            }

            visitingStageIds.Remove(stageId);
            cachedDepths.Add(stageId, unlockDepth);
            return true;
        }

        private readonly DemoSessionState _sessionState;
        private readonly DemoExperienceConfig _config;
        private StageId _finalStageId;
        private bool _hasFinalStage;
    }
}
