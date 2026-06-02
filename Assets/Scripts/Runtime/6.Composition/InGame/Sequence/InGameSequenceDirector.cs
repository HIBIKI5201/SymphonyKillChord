using KillChord.Runtime.View.InGame.Sequence;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Sequence
{
    public class InGameSequenceDirector
    {
        public InGameSequenceDirector(
            StageSequenceView stageSequenceView, 
            StageResultUIView stageResultUIView, 
            IGameplayControllable gameplayControllable)
        {
            _stageSequenceView = stageSequenceView;
            _stageResultUIView = stageResultUIView;
            _gameplayControllable = gameplayControllable;
        }

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
