using KillChord.Runtime.Domain.OutGame.StageSelect;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.OutGame.StageSelect
{
    /// <summary>
    ///     ステージノードの状態を View 向けに変換して渡すプレゼンター。
    /// </summary>
    public sealed class StageNodePresenter
    {
        /// <summary>
        ///     StageNodePresenter を初期化します。
        /// </summary>
        /// <param name="viewModel"> 反映先の ViewModel。</param>
        public StageNodePresenter(IStageNodeViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        /// <summary>
        ///     ステージノードの状態を View 向けに変換して渡します。
        /// </summary>
        /// <param name="node"> 状態を反映するステージノード。</param>
        public void Push(StageNode node)
        {
            var statusView = ConvertStatus(node.Status);
            _viewModel.UpdateStatus(statusView);
        }

        /// <summary>
        ///     Domain 層の StageStatus を View 向けの StageStatusView へ変換します。
        /// </summary>
        /// <param name="status"> Domain 層のステージ状態。</param>
        /// <returns> View 向けのステージ状態。</returns>
        private static StageStatusView ConvertStatus(StageStatus status)
        {
            switch (status)
            {
                case StageStatus.Locked:
                    return StageStatusView.Locked;
                case StageStatus.Unlocked:
                    return StageStatusView.Unlocked;
                case StageStatus.Cleared:
                    return StageStatusView.Cleared;
                default:
                    return StageStatusView.Locked;
            }
        }

        private readonly IStageNodeViewModel _viewModel;
    }
}
