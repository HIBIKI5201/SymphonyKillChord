using KillChord.Runtime.Domain.OutGame.StageSelect;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.StageSelect
{
    /// <summary>
    ///     ステージノードの状態をView向けに変換して渡すプレゼンター。
    ///     ノードの状態変化イベントを購読し、自動でViewに反映します。
    ///     Unlocked 状態への遷移時は接続線アニメーション完了後にViewを更新します。
    /// </summary>
    public sealed class StageNodePresenter : IDisposable
    {
        /// <summary>
        ///     StageNodePresenter を初期化します。
        /// </summary>
        /// <param name="node"> 監視対象のステージノード。</param>
        /// <param name="viewModel"> 反映先のViewModel。</param>
        /// <param name="incomingConnection">
        ///     このノードへの接続線ViewModel。
        ///     接続線アニメーションが不要な場合は null を指定します。
        /// </param>
        public StageNodePresenter(
            StageNode node,
            IStageNodeViewModel viewModel,
            IStageConnectionViewModel incomingConnection)
        {
            _node = node;
            _viewModel = viewModel;
            _incomingConnection = incomingConnection;
            _cts = new CancellationTokenSource();
            _transitionTask = Task.CompletedTask;

            // 初期状態を反映する
            _viewModel.UpdateStatus(ConvertStatus(node.Status));

            // 状態変化を購読する
            _node.OnStatusChanged += HandleStatusChanged;
        }

        /// <summary> 現在実行中の接続線アニメーションタスク。アニメーションがない場合は完了済みタスク。 </summary>
        public Task TransitionTask => _transitionTask;

        /// <summary>
        ///     購読を解除してリソースを解放します。
        /// </summary>
        public void Dispose()
        {
            _node.OnStatusChanged -= HandleStatusChanged;
            _cts.Cancel();
            _cts.Dispose();
        }

        /// <summary>
        ///     ノードの状態が変化したときの処理。
        ///     Unlocked への遷移時は接続線アニメーションを待機してからViewを更新します。
        /// </summary>
        private async void HandleStatusChanged(StageStatus status)
        {
            // Unlocked かつ接続線がある場合はアニメーションタスクを保持して待機する
            if (status == StageStatus.Unlocked && _incomingConnection != null)
            {
                _transitionTask = _incomingConnection.PlayFillAnimationAsync(_cts.Token);
                await _transitionTask;
            }

            // アニメーション完了後（またはアニメーションなしの場合）にViewへ反映する
            _viewModel.UpdateStatus(ConvertStatus(status));
        }

        /// <summary>
        ///     Domain層の StageStatus を View向けの StageStatusView へ変換します。
        /// </summary>
        /// <param name="status"> Domain層のステージ状態。</param>
        /// <returns> View向けのステージ状態。</returns>
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

        private readonly StageNode _node;
        private readonly IStageNodeViewModel _viewModel;
        private readonly IStageConnectionViewModel _incomingConnection;
        private readonly CancellationTokenSource _cts;
        private Task _transitionTask;
    }
}
