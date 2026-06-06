using KillChord.Runtime.Adaptor.OutGame.Scenario;
using KillChord.Runtime.Adaptor.Persistent.Input;
using KillChord.Runtime.View.Persistent.Input;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KillChord.Runtime.View.OutGame.Scenario
{
    /// <summary>
    /// プレイヤー入力をシナリオ操作へ変換する入力ビュー。
    /// </summary>
    public class ScenarioInputView : MonoBehaviour
    {
        /// <summary>
        ///     依存先を初期化する。
        /// </summary>
        public void Initialize(InputController inputController)
        {
            _inputController = inputController;

            if (!ServiceLocator.TryGetInstance(out _playerInputView))
            {
                Debug.LogError($"[{nameof(ScenarioInputView)}] PlayerInputView が取得できませんでした。", this);
                return;
            }

            Subscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_playerInputView == null || _isSubscribed)
            {
                return;
            }

            _playerInputView.OnScenarioAdvanceInput += HandleAdvanceInput;
            _playerInputView.OnScenarioFastForwardInput += HandleFastForwardInput;
            _playerInputView.OnScenarioPauseInput += HandlePauseInput;
            _playerInputView.OnScenarioSkipInput += HandleSkipInput;

            _isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (_playerInputView == null || !_isSubscribed)
            {
                return;
            }

            _playerInputView.OnScenarioAdvanceInput -= HandleAdvanceInput;
            _playerInputView.OnScenarioFastForwardInput -= HandleFastForwardInput;
            _playerInputView.OnScenarioPauseInput -= HandlePauseInput;
            _playerInputView.OnScenarioSkipInput -= HandleSkipInput;

            _isSubscribed = false;
        }

        private void HandleAdvanceInput(InputContext<float> context)
        {
            if (context.Phase != InputActionPhase.Performed)
            {
                return;
            }

            _inputController?.MouseClick();
        }

        private void HandleFastForwardInput(InputContext<float> context)
        {
            if (context.Phase == InputActionPhase.Started ||
                context.Phase == InputActionPhase.Performed)
            {
                _inputController?.SetFastForward(true);
                return;
            }

            if (context.Phase == InputActionPhase.Canceled)
            {
                _inputController?.SetFastForward(false);
            }
        }

        private void HandlePauseInput(InputContext<float> context)
        {
            if (context.Phase != InputActionPhase.Performed)
            {
                return;
            }

            _inputController?.TogglePause();
        }

        private void HandleSkipInput(InputContext<float> context)
        {
            if (context.Phase != InputActionPhase.Performed)
            {
                return;
            }

            _inputController?.Skip();
        }

        private InputController _inputController;
        private PlayerInputView _playerInputView;
        private bool _isSubscribed;
    }
}