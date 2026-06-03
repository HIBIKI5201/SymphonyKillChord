using KillChord.Runtime.View.InGame.Sequence;
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
        /// <param name="stageResultUIView"> ステージの結果を表示するビュー。 </param>
        /// <param name="gameplayControllable"> ゲームプレイの開始と終了を制御するオブジェクト。 </param>
        public InGameSequenceDirector(
            StageSequenceView stageSequenceView,
            StageResultUIView stageResultUIView,
            IGameplayControllable gameplayControllable)
        {
            _stageSequenceView = stageSequenceView;
            _stageResultUIView = stageResultUIView;
            _gameplayControllable = gameplayControllable;
        }

        /// <summary>
        ///    ゲームプレイの開始演出を開始する。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 非同期操作の完了を表すAwaitable。 </returns>
        public async Awaitable StartAsync(CancellationToken cancellationToken)
        {
            _gameplayControllable.StopGameplay();
            _stageResultUIView?.Hide();
            _stageResultUIView?.SetStageStartMessage();

            if (_stageSequenceView != null)
            {
                await _stageSequenceView.PlayStageStartAsync(cancellationToken);
            }

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
            _stageResultUIView?.SetClearMessage();

            if (_stageSequenceView != null)
            {
                await _stageSequenceView.PlayStageClearAsync(cancellationToken);
            }

            _stageResultUIView?.ShowClear();
        }
        /// <summary>
        ///     ゲームプレイのゲームオーバー演出を開始する。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 非同期操作の完了を表すAwaitable。 </returns>
        public async Awaitable GameOverAsync(CancellationToken cancellationToken)
        {
            _gameplayControllable.StopGameplay();
            _stageResultUIView.SetGameOverMessage();

            if (_stageSequenceView != null)
            {
                await _stageSequenceView.PlayGameOverAsync(cancellationToken);
            }

            _stageResultUIView?.ShowGameOver();
        }

        private readonly StageSequenceView _stageSequenceView;
        private readonly StageResultUIView _stageResultUIView;
        private readonly IGameplayControllable _gameplayControllable;
    }
}
