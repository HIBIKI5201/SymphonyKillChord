using KillChord.Runtime.Adaptor.OutGame.Scenario;
using KillChord.Runtime.Adaptor.Persistent.Input;
using KillChord.Runtime.View.Persistent.Input;
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
        /// <param name="inputController"> シナリオ入力コントローラーです。 </param>
        /// <param name="playerInputView"> プレイヤー入力Viewです。 </param>
        public void Initialize(
            ScenarioInputController inputController,
            PlayerInputView playerInputView)
        {
            _inputController = inputController;
            _playerInputView = playerInputView;

            if (_scenarioUIRaycastView == null || _scenarioUIHideView == null)
            {
                Debug.LogError($"[{nameof(ScenarioInputView)}] ScenarioUIRaycastView / ScenarioUIHideView が未設定です。", this);
                enabled = false;
                return;
            }

            if (_playerInputView == null)
            {
                Debug.LogError($"[{nameof(ScenarioInputView)}] PlayerInputView が未設定です。", this);
                enabled = false;
                return;
            }

            Subscribe();
        }

        private void LateUpdate()
        {
            if (_requestShowUI)
            {
                _requestShowUI = false;
                _scenarioUIHideView?.ShowUI();
            }

            if (_requestHideUI)
            {
                _requestHideUI = false;
                _scenarioUIHideView?.HideUI();
            }
        }

        private void OnDisable()
        {
            Unsubscribe();
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
            _playerInputView.OnScenarioAutoInput += HandleAutoAdvanceInput;
            _playerInputView.OnScenarioHideUIInput += HandleHideUIInput;

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
            _playerInputView.OnScenarioAutoInput -= HandleAutoAdvanceInput;
            _playerInputView.OnScenarioHideUIInput -= HandleHideUIInput;

            _isSubscribed = false;
        }

        private void HandleAdvanceInput(InputContext<float> context)
        {
            if (context.Phase != InputActionPhase.Performed)
            {
                return;
            }

            if (_scenarioUIRaycastView != null && _scenarioUIRaycastView.IsPointerOverScenarioUI())
            {
                return;
            }

            if (_scenarioUIHideView != null && _scenarioUIHideView.IsHidden)
            {
                _requestShowUI = true;
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

        private void HandleAutoAdvanceInput(InputContext<float> context)
        {
            if (context.Phase != InputActionPhase.Performed)
            {
                return;
            }
            _inputController?.ToggleAutoAdvance();
        }

        private void HandleHideUIInput(InputContext<float> context)
        {
            if (context.Phase != InputActionPhase.Performed)
            {
                return;
            }

            _requestHideUI = true;
        }

        [SerializeField]
        private ScenarioUIRaycastView _scenarioUIRaycastView;

        [SerializeField]
        private ScenarioUIHideView _scenarioUIHideView;

        private ScenarioInputController _inputController;
        private PlayerInputView _playerInputView;
        private bool _isSubscribed;
        private bool _requestHideUI;
        private bool _requestShowUI;
    }
}
