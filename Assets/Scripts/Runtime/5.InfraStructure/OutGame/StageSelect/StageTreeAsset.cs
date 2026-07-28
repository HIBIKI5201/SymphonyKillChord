using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.Utility.Constant;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.StageSelect
{
    /// <summary>
    ///     ステージツリー全体の定義を保持するアセットクラス。
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(StageTreeAsset),
        menuName = PathConst.CREATE_ASSET_MENU_PATH + "StageSelect/" + nameof(StageTreeAsset))]
    public sealed class StageTreeAsset : ScriptableObject
    {
        /// <summary>
        ///     ステージツリーを生成する。
        /// </summary>
        /// <returns> 生成したステージツリー。</returns>
        public StageTree Create()
        {
            List<StageNode> nodes = new(_stageAssets.Count);
            for (int i = 0; i < _stageAssets.Count; i++)
            {
                StageAssetBase stageAsset = _stageAssets[i];
                if (stageAsset == null)
                {
                    throw new System.InvalidOperationException(
                        $"[{nameof(StageTreeAsset)}] ステージアセットが未設定です。Index: {i}");
                }

                nodes.Add(stageAsset.Create());
            }

            List<StageNodeConnection> connections = new(_bindAssets.Count);
            for (int i = 0; i < _bindAssets.Count; i++)
            {
                StageBindAsset bindAsset = _bindAssets[i];
                if (bindAsset == null)
                {
                    throw new System.InvalidOperationException(
                        $"[{nameof(StageTreeAsset)}] Bindアセットが未設定です。Index: {i}");
                }

                connections.Add(bindAsset.Create());
            }

            return new StageTree(nodes, connections);
        }

        [Header("ステージ一覧")]
        [SerializeField, Tooltip("このツリーに含まれるBattleまたはScenarioステージアセットの一覧。")]
        private List<StageAssetBase> _stageAssets = new();

        [Header("接続一覧")]
        [SerializeField, Tooltip("このツリーに含まれるステージ間Bindアセットの一覧。")]
        private List<StageBindAsset> _bindAssets = new();

#if UNITY_EDITOR
        /// <summary>
        ///     ステージと接続の入力内容を検証する。
        /// </summary>
        private void OnValidate()
        {
            HashSet<StageAssetBase> registeredStages = ValidateStageAssets();
            ValidateBindAssets(registeredStages);
        }

        /// <summary>
        ///     ステージアセット一覧を検証する。
        /// </summary>
        /// <returns> 登録済みステージアセットの集合。</returns>
        private HashSet<StageAssetBase> ValidateStageAssets()
        {
            HashSet<StageAssetBase> registeredStages = new();
            HashSet<int> stageIds = new();
            int tutorialCount = 0;

            for (int i = 0; i < _stageAssets.Count; i++)
            {
                StageAssetBase stageAsset = _stageAssets[i];
                if (stageAsset == null)
                {
                    Debug.LogError(
                        $"[{nameof(StageTreeAsset)}] ステージアセットが未設定です。Index: {i}",
                        this);
                    continue;
                }

                registeredStages.Add(stageAsset);
                if (stageAsset.StageIdValue == 0)
                {
                    Debug.LogError(
                        $"[{nameof(StageTreeAsset)}] StageIdが未設定です。Asset: {stageAsset.name}",
                        this);
                    continue;
                }

                if (!stageIds.Add(stageAsset.StageIdValue))
                {
                    Debug.LogError(
                        $"[{nameof(StageTreeAsset)}] StageIdが重複しています。StageId: {stageAsset.StageIdValue}",
                        this);
                }

                if (stageAsset.IsTutorial)
                {
                    tutorialCount++;
                }
            }

            if (tutorialCount > 1)
            {
                Debug.LogError(
                    $"[{nameof(StageTreeAsset)}] チュートリアルステージが複数設定されています。",
                    this);
            }

            return registeredStages;
        }

        /// <summary>
        ///     Bindアセット一覧を検証する。
        /// </summary>
        /// <param name="registeredStages"> 登録済みステージアセットの集合。</param>
        private void ValidateBindAssets(HashSet<StageAssetBase> registeredStages)
        {
            HashSet<(int FromStageId, int ToStageId)> connections = new();
            HashSet<int> autoAdvanceFromIds = new();
            Dictionary<StageAssetBase, int> incomingCounts = new();
            Dictionary<StageAssetBase, List<StageAssetBase>> outgoingStages = new();

            foreach (StageAssetBase stageAsset in registeredStages)
            {
                incomingCounts.Add(stageAsset, 0);
                outgoingStages.Add(stageAsset, new List<StageAssetBase>());
            }

            for (int i = 0; i < _bindAssets.Count; i++)
            {
                StageBindAsset bindAsset = _bindAssets[i];
                if (bindAsset == null)
                {
                    Debug.LogError(
                        $"[{nameof(StageTreeAsset)}] Bindアセットが未設定です。Index: {i}",
                        this);
                    continue;
                }

                if (!ValidateBindReferences(bindAsset, registeredStages))
                {
                    continue;
                }

                int fromStageId = bindAsset.FromStage.StageIdValue;
                int toStageId = bindAsset.ToStage.StageIdValue;
                if (!connections.Add((fromStageId, toStageId)))
                {
                    Debug.LogError(
                        $"[{nameof(StageTreeAsset)}] Bindが重複しています。Asset: {bindAsset.name}",
                        this);
                    continue;
                }

                outgoingStages[bindAsset.FromStage].Add(bindAsset.ToStage);
                incomingCounts[bindAsset.ToStage]++;

                if (bindAsset.AdvanceMode == StageAdvanceMode.AutoAdvance
                    && !autoAdvanceFromIds.Add(fromStageId))
                {
                    Debug.LogError(
                        $"[{nameof(StageTreeAsset)}] 同じ接続元から複数の自動遷移は設定できません。" +
                        $"FromStage: {bindAsset.FromStage.name}",
                        this);
                }
            }

            ValidateTopology(registeredStages, incomingCounts, outgoingStages);
        }

        /// <summary>
        ///     起点数と循環の有無を検証する。
        /// </summary>
        /// <param name="registeredStages"> 登録済みステージアセットの集合。</param>
        /// <param name="incomingCounts"> ステージ別の入力接続数。</param>
        /// <param name="outgoingStages"> ステージ別の後続ステージ一覧。</param>
        private void ValidateTopology(
            HashSet<StageAssetBase> registeredStages,
            Dictionary<StageAssetBase, int> incomingCounts,
            Dictionary<StageAssetBase, List<StageAssetBase>> outgoingStages)
        {
            if (registeredStages.Count == 0)
            {
                return;
            }

            Queue<StageAssetBase> processingQueue = new();
            foreach (StageAssetBase stageAsset in registeredStages)
            {
                if (incomingCounts[stageAsset] == 0)
                {
                    processingQueue.Enqueue(stageAsset);
                }
            }

            int rootCount = processingQueue.Count;
            if (rootCount == 0)
            {
                Debug.LogError(
                    $"[{nameof(StageTreeAsset)}] 起点ステージがありません。Bindの循環を確認してください。",
                    this);
            }
            else if (rootCount > 1)
            {
                Debug.LogWarning(
                    $"[{nameof(StageTreeAsset)}] 起点ステージが{rootCount}個あります。" +
                    "作戦画面の左端に複数配置されます。",
                    this);
            }

            int processedCount = 0;
            while (processingQueue.Count > 0)
            {
                StageAssetBase currentStage = processingQueue.Dequeue();
                processedCount++;
                List<StageAssetBase> nextStages = outgoingStages[currentStage];
                for (int i = 0; i < nextStages.Count; i++)
                {
                    StageAssetBase nextStage = nextStages[i];
                    incomingCounts[nextStage]--;
                    if (incomingCounts[nextStage] == 0)
                    {
                        processingQueue.Enqueue(nextStage);
                    }
                }
            }

            if (processedCount != registeredStages.Count)
            {
                Debug.LogError(
                    $"[{nameof(StageTreeAsset)}] Bind設定に循環があります。作戦画面を自動配置できません。",
                    this);
            }
        }

        /// <summary>
        ///     Bindの参照先を検証する。
        /// </summary>
        /// <param name="bindAsset"> 検証するBindアセット。</param>
        /// <param name="registeredStages"> 登録済みステージアセットの集合。</param>
        /// <returns> 有効な場合はtrue。</returns>
        private bool ValidateBindReferences(
            StageBindAsset bindAsset,
            HashSet<StageAssetBase> registeredStages)
        {
            if (bindAsset.FromStage == null || bindAsset.ToStage == null)
            {
                Debug.LogError(
                    $"[{nameof(StageTreeAsset)}] BindのFromStageまたはToStageが未設定です。Asset: {bindAsset.name}",
                    this);
                return false;
            }

            if (bindAsset.FromStage == bindAsset.ToStage)
            {
                Debug.LogError(
                    $"[{nameof(StageTreeAsset)}] 自己接続は設定できません。Asset: {bindAsset.name}",
                    this);
                return false;
            }

            if (!registeredStages.Contains(bindAsset.FromStage)
                || !registeredStages.Contains(bindAsset.ToStage))
            {
                Debug.LogError(
                    $"[{nameof(StageTreeAsset)}] StageTreeに未登録のステージがBindから参照されています。" +
                    $"Asset: {bindAsset.name}",
                    this);
                return false;
            }

            return true;
        }
#endif
    }
}
