using KillChord.Runtime.Application.InGame.Mission;
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
        public StageDetailPresenter(
            IStageDetailViewModel viewModel,
            IMissionPreviewProvider missionPreviewProvider)
        {
            _viewModel = viewModel ?? throw new System.ArgumentNullException(nameof(viewModel));
            _missionPreviewProvider = missionPreviewProvider;
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
                    out evaluationDescriptions);
            }

            var subMissionTexts = evaluationDescriptions?.ToArray();

            // TODO: セーブデータから、ミッション達成状況を取得して反映する。

            var dto = new StageDetailDTO(
                def.StageName,
                def.FlavorText,
                def.Reward.SkillBuildPoint,
                def.Reward.SkillUnlockPoint,
                mainMissionText,
                subMissionTexts);

            _viewModel.Apply(in dto);
        }

        private readonly IStageDetailViewModel _viewModel;
        private readonly IMissionPreviewProvider _missionPreviewProvider;
    }
}
