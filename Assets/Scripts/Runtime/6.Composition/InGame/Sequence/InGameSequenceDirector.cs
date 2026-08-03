using KillChord.Runtime.Adaptor.InGame.Result;
using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.View.InGame.Result;
using KillChord.Runtime.View.InGame.Sequence;
using System;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Sequence
{
    /// <summary>
    ///     ゲームプレイの開始と終了の演出を制御するクラス。
    /// </summary>
    public class InGameSequenceDirector
    {
        /// <summary>
        ///    コンストラクタ。
        /// </summary>
        /// <param name="stageSequenceView"> ステージのシーケンスを表示するビュー。 </param>
        /// <param name="stageSequenceMessageView"> ステージの結果を表示するビュー。 </param>
        /// <param name="stageStartFadeView"> ステージ開始時のフェードを表示するビュー。 </param>
        /// <param name="resultView"> ステージリザルトを表示するビュー。 </param>
        /// <param name="resultPresenter"> ステージリザルトのPresenter。 </param>
        /// <param name="stageStartConstraintView"> ステージ開始時の制約を表示するビュー。 </param>
        /// <param name="gameplayControllable"> ゲームプレイの開始と終了を制御するオブジェクト。 </param>
        public InGameSequenceDirector(
            StageSequenceView stageSequenceView,
            StageSequenceMessageView stageSequenceMessageView,
            StageStartFadeView stageStartFadeView,
            StageResultView resultView,
            StageStartConstraintView stageStartConstraintView,
            StageResultPresenter resultPresenter,
            IGameplayControllable gameplayControllable)
        {
            _stageSequenceView = stageSequenceView ?? throw new ArgumentNullException(nameof(stageSequenceView));
            _stageSequenceMessageView = stageSequenceMessageView ?? throw new ArgumentNullException(nameof(stageSequenceMessageView));
            _stageStartFadeView = stageStartFadeView ?? throw new ArgumentNullException(nameof(stageStartFadeView));
            _stageStartConstraintView = stageStartConstraintView ?? throw new ArgumentNullException(nameof(stageStartConstraintView));
            _stageResultView = resultView ?? throw new ArgumentNullException(nameof(resultView));
            _stageResultPresenter = resultPresenter ?? throw new ArgumentNullException(nameof(resultPresenter));
            _gameplayControllable = gameplayControllable ?? throw new ArgumentNullException(nameof(gameplayControllable));
        }

        /// <summary>
        ///    ゲームプレイの開始演出を開始する。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 非同期操作の完了を表すAwaitable。 </returns>
        public void Start()
        {
            if (_isStartPlaying)
            {
                return;
            }

            _isStartPlaying = true;
            _isTimelineCompleted = false;
            _isCameraCompleted = false;
            _isCameraPrepared = false;

            _gameplayControllable.StopGameplay();
            _stageResultView.Hide();
            _stageSequenceMessageView?.Hide();
            _stageSequenceMessageView?.SetStageStartMessage();
            _stageStartFadeView.ShowBlackImmediate();

            _isCameraCompleted =
                !_isCameraPrepared;

            _stageSequenceView.PlayStageStart(HandleTimelineCompleted);
            _stageStartFadeView.PlayFadeOut(HandleFadeCompleted);
        }

        /// <summary>
        ///     ゲームプレイの開始演出をキャンセルする。
        /// </summary>
        public void Cancel()
        {
            if (!_isStartPlaying)
            {
                return;
            }

            _isStartPlaying = false;

            _stageStartFadeView.HideImmediate();
            _stageSequenceView.CancelStageStart();
            _stageSequenceMessageView?.Hide();

            // SourceのAddはModule(Ready)で行う。開始演出を中断したのでここで解放する。
            _stageStartConstraintView.RemoveSource();
        }

        /// <summary>
        ///     ゲームプレイのクリア演出を開始する。
        /// </summary>
        /// <param name="evaluationResult"> 確定済みのミッション評価結果です。 </param>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 非同期操作の完了を表すAwaitable。 </returns>
        public async Awaitable ClearAsync(
            MissionEvaluationResult evaluationResult,
            CancellationToken cancellationToken)
        {
            Cancel();

            _gameplayControllable.StopGameplay();
            _stageSequenceMessageView?.ShowClear();

            if (_stageSequenceView != null)
            {
                await _stageSequenceView.PlayStageClearAsync(cancellationToken);
            }

            _stageSequenceMessageView?.Hide();
            _stageResultPresenter.PresentVictory(evaluationResult);
            _stageResultView?.Show();
        }
        /// <summary>
        ///     ゲームプレイのゲームオーバー演出を開始する。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 非同期操作の完了を表すAwaitable。 </returns>
        public async Awaitable GameOverAsync(CancellationToken cancellationToken)
        {
            Cancel();

            _gameplayControllable.StopGameplay();
            _stageSequenceMessageView?.ShowGameOver();

            if (_stageSequenceView != null)
            {
                await _stageSequenceView.PlayGameOverAsync(cancellationToken);
            }

            _stageSequenceMessageView?.Hide();
            _stageResultPresenter.PresentDefeat();
            _stageResultView?.Show();
        }

        private readonly StageSequenceView _stageSequenceView;
        private readonly StageSequenceMessageView _stageSequenceMessageView;
        private readonly StageStartFadeView _stageStartFadeView;
        private readonly StageResultView _stageResultView;
        private readonly StageResultPresenter _stageResultPresenter;
        private readonly StageStartConstraintView _stageStartConstraintView;
        private readonly IGameplayControllable _gameplayControllable;

        private bool _isStartPlaying;
        private bool _isCameraPrepared;
        private bool _isCameraCompleted;
        private bool _isTimelineCompleted;

        /// <summary>
        ///     ゲームプレイ開始演出の完了条件を確認し、すべての条件が満たされていればゲームプレイを開始します。
        /// </summary>
        private void TryCompleteStart()
        {
            if (!_isStartPlaying
                || !_isCameraCompleted
                || !_isTimelineCompleted)
            {
                return;
            }

            _isStartPlaying = false;

            _stageStartFadeView.HideImmediate();
            _stageSequenceMessageView.Hide();
            _gameplayControllable.StartGameplay();

            // SourceのAddはModule(Ready)で行う。開始演出が完了したのでここで解放する。
            _stageStartConstraintView.RemoveSource();
        }

        /// <summary>
        ///    ゲームプレイ開始演出のタイムラインの完了を記録します。
        /// </summary>
        private void HandleTimelineCompleted()
        {
            if (!_isStartPlaying)
            {
                return;
            }

            _isTimelineCompleted = true;
            TryCompleteStart();

        }

        /// <summary>
        ///     フェードアウト完了後にカメラ周回を開始します。
        /// </summary>
        private void HandleFadeCompleted()
        {
            if (!_isStartPlaying)
            {
                return;
            }

            if (!_isCameraPrepared)
            {
                _isCameraCompleted = true;
                TryCompleteStart();
                return;
            }
        }

        /// <summary>
        ///     カメラ演出の完了を記録します。
        /// </summary>
        private void HandleCameraCompleted()
        {
            if (!_isStartPlaying)
            {
                return;
            }

            _isCameraCompleted = true;
            TryCompleteStart();
        }
    }
}
