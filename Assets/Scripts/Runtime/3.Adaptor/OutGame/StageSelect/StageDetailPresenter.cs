using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Application.OutGame.Resource;
using KillChord.Runtime.Domain.OutGame.Resource;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using System.Collections.Generic;
using System.Linq;

namespace KillChord.Runtime.Adaptor.OutGame.StageSelect
{
    /// <summary>
    ///     ステージノードの情報を View  向けに変換して渡すプレゼンター。
    /// </summary>
    public sealed class StageDetailPresenter
    {
        /// <summary>
        ///     StageDetailPresenter を初期化します。
        /// </summary>
        /// <param name="viewModel"> 反映先の ViewModel。</param>
        /// <param name="missionPreviewProvider"> ミッションテキストプレビューの解決に使うプロバイダー。 </param>
        /// <param name="subMissionAchievementResolver"> サブミッションの達成状況を解決するリゾルバー。 </param>
        /// <param name="resourceRepository">
        ///     報酬リソースの表示名の解決に使うリポジトリ。
        ///     null の場合は既知のリソースのみ既定の名前で表示します。
        /// </param>
        public StageDetailPresenter(
            IStageDetailViewModel viewModel,
            IMissionPreviewProvider missionPreviewProvider,
            SubMissionAchievementResolver subMissionAchievementResolver,
            IGameResourceDefinitionRepository resourceRepository = null)
        {
            _viewModel = viewModel ?? throw new System.ArgumentNullException(nameof(viewModel));
            _missionPreviewProvider = missionPreviewProvider;
            _subMissionAchievementResolver = subMissionAchievementResolver;
            _resourceRepository = resourceRepository;
        }

        /// <summary>
        ///     ステージノードの情報を DTO へ変換して ViewModel に渡します。
        /// </summary>
        /// <param name="node"> 詳細を表示するステージノード。</param>
        public void Push(StageNode node)
        {
            if (node == null)
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogWarning($"[{nameof(StageDetailPresenter)}] node が null です。");
#endif
                return;
            }

            var def = node.Definition;

            // シナリオパートはミッションテキストなし
            BattleStageDefinition battleDefinition = def as BattleStageDefinition;
            string mainMissionText = null;
            IReadOnlyList<string> evaluationDescriptions = null;
            if (battleDefinition != null && _missionPreviewProvider != null)
            {
                _missionPreviewProvider.TryGetPreview(
                    battleDefinition.MissionId,
                    out mainMissionText,
                    out evaluationDescriptions,
                    out _);
            }

            var subMissionTexts = evaluationDescriptions?.ToArray();
            var subMissionCleared = _subMissionAchievementResolver?.Resolve(node);

            var dto = new StageDetailDTO(
                def.StageName,
                def.FlavorText,
                CreateRewardViewData(def.FirstClearReward),
                CreateRewardViewData(def.ClearReward),
                mainMissionText,
                subMissionTexts,
                subMissionCleared);

            _viewModel.Apply(in dto);
        }

        private const string RESEARCH_POINT_DEFAULT_NAME = "研究ポイント";
        private const string SKILL_LEVELUP_POINT_DEFAULT_NAME = "改造ポイント";
        private const string UNKNOWN_RESOURCE_NAME = "不明なリソース";

        private readonly IStageDetailViewModel _viewModel;
        private readonly IMissionPreviewProvider _missionPreviewProvider;
        private readonly SubMissionAchievementResolver _subMissionAchievementResolver;
        private readonly IGameResourceDefinitionRepository _resourceRepository;

        /// <summary>
        ///     報酬を画面表示用のデータへ変換します。
        /// </summary>
        /// <param name="reward"> 変換する報酬。</param>
        /// <returns> 画面表示用の報酬データの一覧。</returns>
        private StageRewardViewData[] CreateRewardViewData(StageReward reward)
        {
            IReadOnlyList<GameResourceAmount> items = reward.Items;
            StageRewardViewData[] viewData = new StageRewardViewData[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                GameResourceAmount item = items[i];
                viewData[i] = new StageRewardViewData(ResolveDisplayName(item.ResourceId), item.Amount);
            }

            return viewData;
        }

        /// <summary>
        ///     リソースの表示名を解決します。
        /// </summary>
        /// <param name="resourceId"> 表示名を解決するリソースID。</param>
        /// <returns> リソースの表示名。</returns>
        private string ResolveDisplayName(GameResourceId resourceId)
        {
            if (_resourceRepository != null
                && _resourceRepository.TryGetDefinition(resourceId, out GameResourceDefinition definition)
                && !string.IsNullOrEmpty(definition.DisplayName))
            {
                return definition.DisplayName;
            }

            // リソース定義アセットが未作成でも既存ポイントの報酬が読めるよう、既知のリソースは既定名で表示する。
            if (resourceId.Equals(GameResourceIds.ResearchPoint))
            {
                return RESEARCH_POINT_DEFAULT_NAME;
            }

            if (resourceId.Equals(GameResourceIds.SkillLevelupPoint))
            {
                return SKILL_LEVELUP_POINT_DEFAULT_NAME;
            }

            return UNKNOWN_RESOURCE_NAME;
        }
    }
}
