using KillChord.Runtime.Domain.OutGame.StageSelect;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.StageSelect
{
    /// <summary>
    ///     ステージノードの状態を View 向けに変換して渡すプレゼンター。
    ///     ノードの状態変化イベントを購読し、自動で View に反映します。
    ///     Unlocked 状態への遷移時は接続線アニメーション完了後に View を更新します。
    ///     複数ノードにまたがる演出の順序は <see cref="StageNodeAnimationSequencer"/> が保証します。
    /// </summary>
    public sealed class StageNodePresenter : IDisposable
    {
        /// <summary>
        ///     StageNodePresenter を初期化します。
        /// </summary>
        /// <param name="node"> 監視対象のステージノード。</param>
        /// <param name="viewModel"> 反映先の ViewModel。</param>
        /// <param name="incomingConnections">
        ///     このノードへの接続線 ViewModel一覧。
        ///     接続線アニメーションが不要な場合は null を指定します。
        /// </param>
        /// <param name="sequencer">
        ///     全 Presenter で共有するアニメーションシーケンサー。
        /// </param>
        public StageNodePresenter(
            StageNode node,
            IStageNodeViewModel viewModel,
            IReadOnlyList<IStageConnectionViewModel> incomingConnections,
            StageNodeAnimationSequencer sequencer)
        {
            _node = node;
            _viewModel = viewModel;
            _incomingConnections = incomingConnections;
            _sequencer = sequencer;
            _cts = new CancellationTokenSource();
            _transitionTask = Task.CompletedTask;

            // 初期状態を反映する
            _viewModel.UpdateStatus(ConvertStatus(node.Status));
            if ((node.Status == StageStatus.Unlocked || node.Status == StageStatus.Cleared)
                && _incomingConnections != null)
            {
                for (int i = 0; i < _incomingConnections.Count; i++)
                {
                    _incomingConnections[i].ApplyFilledState();
                }
            }

            // 状態変化を購読する
            _node.OnStatusChanged += HandleStatusChanged;
        }

        /// <summary> 現在実行中または最後に登録された演出タスク。 </summary>
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
        ///     シーケンサーへ処理を登録し、FIFO 順での実行を保証します。
        /// </summary>
        private void HandleStatusChanged(StageStatus status)
        {
            _transitionTask = _sequencer.Enqueue(token => ProcessStatusAsync(status, token), _cts.Token);
        }

        /// <summary>
        ///     接続線アニメーションを再生し、完了後に View へ状態を反映します。
        /// </summary>
        /// <param name="status"> 変化後のステージ状態。</param>
        /// <param name="token"> キャンセルトークン。</param>
        private async Task ProcessStatusAsync(StageStatus status, CancellationToken token)
        {
            // Unlocked かつ接続線がある場合は接続線アニメーションを再生する
            if (status == StageStatus.Unlocked
                && _incomingConnections != null
                && _incomingConnections.Count > 0)
            {
                Task[] animationTasks = new Task[_incomingConnections.Count];
                for (int i = 0; i < _incomingConnections.Count; i++)
                {
                    animationTasks[i] = _incomingConnections[i].PlayFillAnimationAsync(token);
                }

                await Task.WhenAll(animationTasks);
            }

            // アニメーション完了後（またはアニメーションなしの場合）に View へ反映する
            _viewModel.UpdateStatus(ConvertStatus(status));
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

        private readonly StageNode _node;
        private readonly IStageNodeViewModel _viewModel;
        private readonly IReadOnlyList<IStageConnectionViewModel> _incomingConnections;
        private readonly StageNodeAnimationSequencer _sequencer;
        private readonly CancellationTokenSource _cts;
        private Task _transitionTask;
    }
}
