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
                }

                if (bindAsset.AdvanceMode == StageAdvanceMode.AutoAdvance
                    && !autoAdvanceFromIds.Add(fromStageId))
                {
                    Debug.LogError(
                        $"[{nameof(StageTreeAsset)}] 同じ接続元から複数の自動遷移は設定できません。" +
                        $"FromStage: {bindAsset.FromStage.name}",
                        this);
                }
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
