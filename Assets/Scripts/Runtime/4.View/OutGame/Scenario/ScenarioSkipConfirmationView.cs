using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.UI;

namespace KillChord.Runtime.View.OutGame.Scenario
{
    /// <summary>
    ///     シナリオスキップの確認画面と、開始入力の解放待ちを管理する。
    /// </summary>
    public sealed class ScenarioSkipConfirmationView : MonoBehaviour
    {
        /// <summary> スキップが確定されたときの通知。 </summary>
        public event Action OnConfirmed;
        /// <summary> 確認がキャンセルされたときの通知。 </summary>
        public event Action OnCancelled;
        /// <summary> 確認画面が表示中かを示す。 </summary>
        public bool IsOpen => _dialogRoot != null && _dialogRoot.activeSelf;

        /// <summary>
        ///     プレイヤーと同じUI入力アクションを使って確認画面を初期化する。
        /// </summary>
        public void Initialize(PlayerInput playerInput, InputSystemUIInputModule uiInputModule)
        {
            Hide();
            UnsubscribeCancel();
            _skipAction = playerInput.actions.FindAction(SKIP_ACTION_NAME, true);
            _cancelAction = uiInputModule.cancel.action;
            _submitAction = uiInputModule.submit.action;
            _clickAction = uiInputModule.leftClick.action;
            if (isActiveAndEnabled) { _cancelAction.performed += HandleCancelInputHandler; }
        }

        /// <summary>
        ///     確認画面を開き、現在押されている開始入力が離されるまで操作を保留する。
        /// </summary>
        public void Show()
        {
            if (IsOpen) { return; }
            _previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            _openingControls.Clear();
            CapturePressedControls(_skipAction);
            CapturePressedControls(_cancelAction);
            CapturePressedControls(_submitAction);
            CapturePressedControls(_clickAction);
            _openedFrame = Time.frameCount;
            _isReady = false;
            _skipButton.interactable = false;
            _cancelButton.interactable = false;
            EventSystem.current?.SetSelectedGameObject(null);
            _dialogRoot.SetActive(true);
        }

        /// <summary>
        ///     確認画面を閉じ、表示前のUI選択を復元する。
        /// </summary>
        public void Hide()
        {
            if (!IsOpen) { return; }
            _dialogRoot.SetActive(false);
            _isReady = false;
            _openingControls.Clear();
            GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            if (selected == null || selected.transform.IsChildOf(_dialogRoot.transform))
            {
                EventSystem.current?.SetSelectedGameObject(
                    _previousSelection != null && _previousSelection.activeInHierarchy ? _previousSelection : null);
            }
            _previousSelection = null;
        }

        private const string SKIP_ACTION_NAME = "Scenario/Skip";

        [SerializeField, Tooltip("背景遮蔽と確認パネルを含む表示ルート。")]
        private GameObject _dialogRoot;
        [SerializeField, Tooltip("スキップを確定するボタン。")]
        private UnityEngine.UI.Button _skipButton;
        [SerializeField, Tooltip("確認を閉じる初期選択ボタン。")]
        private UnityEngine.UI.Button _cancelButton;

        private readonly List<ButtonControl> _openingControls = new();
        private InputAction _skipAction;
        private InputAction _cancelAction;
        private InputAction _submitAction;
        private InputAction _clickAction;
        private GameObject _previousSelection;
        private int _openedFrame;
        private bool _isReady;

        /// <summary>
        ///     ボタンのクリック通知を登録する。
        /// </summary>
        private void Awake()
        {
            _skipButton.onClick.AddListener(HandleConfirmClickedHandler);
            _cancelButton.onClick.AddListener(HandleCancelClickedHandler);
        }

        /// <summary>
        ///     開始入力の解放後にボタンを有効化し、選択を確認画面内に限定する。
        /// </summary>
        private void LateUpdate()
        {
            if (!IsOpen) { return; }
            if (!_isReady)
            {
                if (Time.frameCount <= _openedFrame) { return; }
                foreach (ButtonControl control in _openingControls)
                {
                    if (control.device.added && control.isPressed) { return; }
                }
                _isReady = true;
                _skipButton.interactable = true;
                _cancelButton.interactable = true;
            }
            GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            if (selected != _skipButton.gameObject && selected != _cancelButton.gameObject)
            {
                EventSystem.current?.SetSelectedGameObject(_cancelButton.gameObject);
            }
        }

        /// <summary>
        ///     無効化時に確認を閉じる。
        /// </summary>
        private void OnDisable()
        {
            Hide();
            UnsubscribeCancel();
        }

        /// <summary>
        ///     再有効化時にUIの戻る入力を購読する。
        /// </summary>
        private void OnEnable()
        {
            if (_cancelAction != null) { _cancelAction.performed += HandleCancelInputHandler; }
        }

        /// <summary>
        ///     破棄時に入力とボタンの購読を解除する。
        /// </summary>
        private void OnDestroy()
        {
            UnsubscribeCancel();
            if (_skipButton != null) { _skipButton.onClick.RemoveListener(HandleConfirmClickedHandler); }
            if (_cancelButton != null) { _cancelButton.onClick.RemoveListener(HandleCancelClickedHandler); }
        }

        /// <summary>
        ///     有効な決定操作を確定通知へ変換する。
        /// </summary>
        private void HandleConfirmClickedHandler()
        {
            if (IsOpen && _isReady) { OnConfirmed?.Invoke(); }
        }

        /// <summary>
        ///     有効なキャンセル操作をキャンセル通知へ変換する。
        /// </summary>
        private void HandleCancelClickedHandler()
        {
            if (IsOpen && _isReady) { OnCancelled?.Invoke(); }
        }

        /// <summary>
        ///     UIの戻る入力をキャンセル操作へ変換する。
        /// </summary>
        private void HandleCancelInputHandler(InputAction.CallbackContext context)
        {
            HandleCancelClickedHandler();
        }

        /// <summary>
        ///     現在押されているボタンを開始入力として記録する。
        /// </summary>
        private void CapturePressedControls(InputAction action)
        {
            if (action == null) { return; }
            foreach (InputControl control in action.controls)
            {
                if (control is ButtonControl button && button.isPressed && !_openingControls.Contains(button))
                {
                    _openingControls.Add(button);
                }
            }
        }

        /// <summary>
        ///     UIの戻る入力の購読を解除する。
        /// </summary>
        private void UnsubscribeCancel()
        {
            if (_cancelAction != null) { _cancelAction.performed -= HandleCancelInputHandler; }
        }
    }
}
