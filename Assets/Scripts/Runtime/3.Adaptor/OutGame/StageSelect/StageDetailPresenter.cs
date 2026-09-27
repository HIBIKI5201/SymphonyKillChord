using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Domain.OutGame.Resource;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.Domain.Persistent.Savedata;
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
        /// <param name="saveData"> 現在の研究ポイント/改造ポイントの所持数を参照するためのセーブデータ。 </param>
        public StageDetailPresenter(
            IStageDetailViewModel viewModel,
            IMissionPreviewProvider missionPreviewProvider,
            SubMissionAchievementResolver subMissionAchievementResolver,
            SaveData saveData)
        {
            _viewModel = viewModel ?? throw new System.ArgumentNullException(nameof(viewModel));
            _missionPreviewProvider = missionPreviewProvider;
            _subMissionAchievementResolver = subMissionAchievementResolver;
            _saveData = saveData;
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

            ResourceInventoryData inventory = _saveData?.ResourceInventory;
            int currentSkillUnlockPoint = inventory?.GetAmount(GameResourceIds.ResearchPoint) ?? 0;
            int currentSkillBuildPoint = inventory?.GetAmount(GameResourceIds.SkillLevelupPoint) ?? 0;

            // 初回報酬・成功報酬のそれぞれで、研究ポイントと改造ポイントの両方を集計する。
            var dto = new StageDetailDTO(
                def.StageName,
                def.FlavorText,
                currentSkillUnlockPoint,
                currentSkillBuildPoint,
                SumAmount(def.FirstClearReward, GameResourceIds.ResearchPoint),
                SumAmount(def.FirstClearReward, GameResourceIds.SkillLevelupPoint),
                SumAmount(def.ClearReward, GameResourceIds.ResearchPoint),
                SumAmount(def.ClearReward, GameResourceIds.SkillLevelupPoint),
                mainMissionText,
                subMissionTexts,
                subMissionCleared);

            _viewModel.Apply(in dto);
        }

        private readonly IStageDetailViewModel _viewModel;
        private readonly IMissionPreviewProvider _missionPreviewProvider;
        private readonly SubMissionAchievementResolver _subMissionAchievementResolver;
        private readonly SaveData _saveData;

        /// <summary>
        ///     報酬に含まれる指定リソースの数量を合計します。
        /// </summary>
        /// <param name="reward"> 集計する報酬。</param>
        /// <param name="resourceId"> 集計対象のリソースID。</param>
        /// <returns> 指定リソースの数量の合計。</returns>
        private static int SumAmount(StageReward reward, GameResourceId resourceId)
        {
            int total = 0;
            IReadOnlyList<GameResourceAmount> items = reward.Items;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].ResourceId.Equals(resourceId))
                {
                    total = checked(total + items[i].Amount);
                }
            }

            return total;
        }
    }
}
