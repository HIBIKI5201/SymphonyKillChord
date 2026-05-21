using UnityEngine;
using KillChord.Runtime.Domain.OutGame.StageSelect;

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
        public StageDetailPresenter(IStageDetailViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        /// <summary>
        ///     ステージノードの情報を DTO へ変換して ViewModel に渡します。
        /// </summary>
        /// <param name="node"> 詳細を表示するステージノード。</param>
        public void Push(StageNode node)
        {
            var def = node.Definition;

            // シナリオパートはミッションテキストなし
            var mainMissionText = def.StageType == StageType.Battle && def.MissionDefinition != null
                ? def.MissionDefinition.MainMissionText
                : null;

            var dto = new StageDetailDTO(
                def.StageName,
                def.FlavorText,
                def.Reward.SkillBuildPoint,
                def.Reward.SkillUnlockPoint,
                mainMissionText);

            _viewModel.Apply(in dto);
        }

        private readonly IStageDetailViewModel _viewModel;
    }
}
