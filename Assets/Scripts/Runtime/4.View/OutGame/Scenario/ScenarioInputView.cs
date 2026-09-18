using KillChord.Runtime.Adaptor.OutGame.Scenario;
using KillChord.Runtime.Adaptor.Persistent.Input;
using KillChord.Runtime.View.Persistent.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

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
            PlayerInputView playerInputView,
            ScenarioViewModel viewModel)
        {
            ClearSkipConfirmation();
            Unsubscribe();
            _inputController = inputController;
            _playerInputView = playerInputView;
            _viewModel = viewModel;
            if (_autoButton == null)
            {
                Debug.LogError($"[{nameof(ScenarioInputView)}] AutoButton が未設定です。", this);
                enabled = false;
                return;
            }
            _autoButtonDefaultColors = _autoButton.colors;
            UpdateAutoButtonColor();


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

            if (_skipConfirmationView == null)
            {
                Debug.LogError($"[{nameof(ScenarioInputView)}] スキップ確認Viewが未設定です。", this);
                enabled = false;
                return;
            }
            _skipConfirmationView.Initialize(
                _playerInputView.GetComponent<PlayerInput>(),
                _playerInputView.GetComponent<InputSystemUIInputModule>());
            _skipAction = _playerInputView.GetComponent<PlayerInput>().actions.FindAction("Scenario/Skip", true);

            if (isActiveAndEnabled)
            {
                Subscribe();
            }
        }

        private void LateUpdate()
        {
            if (_requestShowUI)
            {
                _requestShowUI = false;
                _scenarioUIHideView?.ShowUI();
                _viewModel?.RefreshText();
            }

            if (_requestHideUI)
            {
                _requestHideUI = false;
                _scenarioUIHideView?.HideUI();
            }
        }

        /// <summary>
        /// 再生開始前にUIを復元し、最新文字状態を反映する。
        /// </summary>
        public void RestoreUIForPlayback()
        {
            ClearSkipConfirmation();
            _requestHideUI = false;
            _requestShowUI = false;
            _scenarioUIHideView?.RestoreForPlayback();
            _viewModel?.RefreshText();
        }

        /// <summary>
        ///     確認表示と一時停止状態を片付け、終了操作の次送りへの流入を防ぐ。
        /// </summary>
        public void ClearSkipConfirmation()
        {
            _blockedInputFrame = Time.frameCount;
            _ignoreSkipUntilRelease = IsSkipControlPressed();
            _skipConfirmationView?.Hide();
            _inputController?.CancelSkipConfirmation();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            ClearSkipConfirmation();
            Unsubscribe();
        }

        private void OnDestroy()
        {
            ClearSkipConfirmation();
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
            if (_skipConfirmationView != null)
            {
                _skipConfirmationView.OnConfirmed += HandleSkipConfirmedHandler;
                _skipConfirmationView.OnCancelled += HandleSkipCancelledHandler;
            }
            if (_viewModel != null) { _viewModel.OnScenarioCompleted += HandleScenarioCompletedHandler; }

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
            if (_skipConfirmationView != null)
            {
                _skipConfirmationView.OnConfirmed -= HandleSkipConfirmedHandler;
                _skipConfirmationView.OnCancelled -= HandleSkipCancelledHandler;
            }
            if (_viewModel != null) { _viewModel.OnScenarioCompleted -= HandleScenarioCompletedHandler; }

            _isSubscribed = false;
        }

        private void HandleAdvanceInput(InputContext<float> context)
        {
            if (IsScenarioInputBlocked()) { return; }
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
            if (context.Phase != InputActionPhase.Canceled && IsScenarioInputBlocked()) { return; }
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
            if (IsScenarioInputBlocked()) { return; }
            if (context.Phase != InputActionPhase.Performed)
            {
                return;
            }

            _inputController?.TogglePause();
        }

        private void HandleSkipInput(InputContext<float> context)
        {
            if (context.Phase == InputActionPhase.Canceled)
            {
                _ignoreSkipUntilRelease = IsSkipControlPressed();
                return;
            }
            if (_ignoreSkipUntilRelease) { return; }
            if (IsScenarioInputBlocked()) { return; }
            if (context.Phase != InputActionPhase.Performed)
            {
                return;
            }

            if (_inputController != null && _inputController.BeginSkipConfirmation())
            {
                _requestHideUI = false;
                _requestShowUI = false;
                _skipConfirmationView.Show();
            }
        }

        private void HandleAutoAdvanceInput(InputContext<float> context)
        {
            if (IsScenarioInputBlocked()) { return; }
            if (context.Phase != InputActionPhase.Performed)
            {
                return;
            }
            _inputController?.ToggleAutoAdvance();
            UpdateAutoButtonColor();
        }

        private void HandleHideUIInput(InputContext<float> context)
        {
            if (IsScenarioInputBlocked()) { return; }
            if (context.Phase != InputActionPhase.Performed)
            {
                return;
            }

            _requestHideUI = true;
        }

        /// <summary>
        ///     確定操作をスキップ処理へ引き渡し、確認画面を閉じる。
        /// </summary>
        private void HandleSkipConfirmedHandler()
        {
            _blockedInputFrame = Time.frameCount;
            _skipConfirmationView.Hide();
            _inputController?.ConfirmSkip();
        }

        /// <summary>
        ///     キャンセル操作で確認を解除する。
        /// </summary>
        private void HandleSkipCancelledHandler()
        {
            ClearSkipConfirmation();
        }

        /// <summary>
        ///     再生終了時に確認状態を解除する。
        /// </summary>
        private void HandleScenarioCompletedHandler(bool skipped)
        {
            ClearSkipConfirmation();
        }

        /// <summary>
        ///     確認中と確認を閉じたフレームのシナリオ入力を抑止する。
        /// </summary>
        private bool IsScenarioInputBlocked()
        {
            return (_inputController != null && _inputController.IsSkipConfirmationOpen)
                || _blockedInputFrame == Time.frameCount;
        }

        /// <summary>
        ///     Action通知の順序に依存せず、スキップに割り当てられたボタンの押下を確認する。
        /// </summary>
        private bool IsSkipControlPressed()
        {
            if (_skipAction == null) { return false; }
            foreach (InputControl control in _skipAction.controls)
            {
                if (control is ButtonControl button && button.device.added && button.isPressed)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     自動送りの有効状態をAutoボタンの色へ反映する。
        /// </summary>
        private void UpdateAutoButtonColor()
        {
            if (_autoButton == null)
            {
                return;
            }

            ColorBlock colors = _autoButtonDefaultColors;
            if (_inputController != null && _inputController.IsAutoAdvance)
            {
                colors.normalColor = _autoEnabledColor;
                colors.highlightedColor = _autoEnabledColor;
                colors.selectedColor = _autoEnabledColor;
            }
            _autoButton.colors = colors;
        }

        [SerializeField, Tooltip("シナリオの非表示やフェードから独立したスキップ確認画面。")]
        private ScenarioSkipConfirmationView _skipConfirmationView;

        [SerializeField]
        private ScenarioUIRaycastView _scenarioUIRaycastView;

        [SerializeField]
        private ScenarioUIHideView _scenarioUIHideView;

        [SerializeField, Tooltip("自動送りの有効状態を色で示すAutoボタン。")]
        private Button _autoButton;

        [SerializeField, Tooltip("自動送りが有効な時のAutoボタン色。")]
        private Color _autoEnabledColor = Color.cyan;

        private ScenarioInputController _inputController;
        private PlayerInputView _playerInputView;
        private ScenarioViewModel _viewModel;
        private bool _isSubscribed;
        private bool _requestHideUI;
        private bool _requestShowUI;
        private int _blockedInputFrame = -1;
        private InputAction _skipAction;
        private bool _ignoreSkipUntilRelease;
        private ColorBlock _autoButtonDefaultColors;
    }
}
