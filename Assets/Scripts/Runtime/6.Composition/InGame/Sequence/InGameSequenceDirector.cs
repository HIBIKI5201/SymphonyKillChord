using KillChord.Runtime.Adaptor.InGame.Result;
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
        /// <param name="gameplayControllable"> ゲームプレイの開始と終了を制御するオブジェクト。 </param>
        public InGameSequenceDirector(
            StageSequenceView stageSequenceView,
            StageSequenceMessageView stageSequenceMessageView,
            StageResultView resultView,
            StageResultPresenter resultPresenter,
            IGameplayControllable gameplayControllable)
        {
            _stageSequenceView = stageSequenceView;
            _stageSequenceMessageView = stageSequenceMessageView;

            _stageResultView = resultView ?? throw new ArgumentNullException(nameof(resultView));
            _stageResultPresenter = resultPresenter ?? throw new ArgumentNullException(nameof(resultPresenter));
            _gameplayControllable = gameplayControllable ?? throw new ArgumentNullException(nameof(gameplayControllable));
        }

        /// <summary>
        ///    ゲームプレイの開始演出を開始する。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 非同期操作の完了を表すAwaitable。 </returns>
        public async Awaitable StartAsync(CancellationToken cancellationToken)
        {
            _gameplayControllable.StopGameplay();
            _stageResultView.Hide();
            _stageSequenceMessageView?.Hide();
            _stageSequenceMessageView?.SetStageStartMessage();

            if (_stageSequenceView != null)
            {
                await _stageSequenceView.PlayStageStartAsync(cancellationToken);
            }

            _stageSequenceMessageView?.Hide();
            _gameplayControllable.StartGameplay();
        }

        /// <summary>
        ///     ゲームプレイのクリア演出を開始する。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 非同期操作の完了を表すAwaitable。 </returns>
        public async Awaitable ClearAsync(CancellationToken cancellationToken)
        {
            _gameplayControllable.StopGameplay();
            _stageSequenceMessageView?.ShowClear();

            if (_stageSequenceView != null)
            {
                await _stageSequenceView.PlayStageClearAsync(cancellationToken);
            }

            _stageSequenceMessageView?.Hide();
            _stageResultPresenter.PresentVictory();
            _stageResultView?.Show();
        }
        /// <summary>
        ///     ゲームプレイのゲームオーバー演出を開始する。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 非同期操作の完了を表すAwaitable。 </returns>
        public async Awaitable GameOverAsync(CancellationToken cancellationToken)
        {
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
        private readonly StageResultView _stageResultView;
        private readonly StageResultPresenter _stageResultPresenter;
        private readonly IGameplayControllable _gameplayControllable;
    }
}
