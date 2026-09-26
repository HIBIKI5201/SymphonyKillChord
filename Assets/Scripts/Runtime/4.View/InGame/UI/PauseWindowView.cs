using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace KillChord.Runtime.View.InGame.UI
{
    /// <summary>
    ///     ポーズ中に表示するウィンドウのView。
    ///     ボタン操作をイベントとして通知するだけで、実際の処理は持たない。
    /// </summary>
    public class PauseWindowView : MonoBehaviour
    {
        /// <summary> ステージのリスタートが要求された時に発火するイベント。 </summary>
        public event Action OnRestartRequested;

        /// <summary> タイトルへ戻ることが要求された時に発火するイベント。 </summary>
        public event Action OnReturnToTitleRequested;

        /// <summary> ポーズを取り消して戦闘へ戻ることが要求された時に発火するイベント。 </summary>
        public event Action OnCancelRequested;

        /// <summary>
        ///     既存UI入力モジュールの取消操作を購読します。
        /// </summary>
        /// <param name="inputModule"> 決定・取消を管理する既存UI入力モジュールです。 </param>
        public void BindCancelInput(InputSystemUIInputModule inputModule)
        {
            UnbindCancelInput();
            _inputModule = inputModule;
            _cancelAction = inputModule != null ? inputModule.cancel?.action : null;
            if (isActiveAndEnabled && _cancelAction != null)
            {
                _cancelAction.performed += HandleCancelInputHandler;
            }
        }

        /// <summary>
        ///     取消操作の購読と入力モジュール参照を解除します。
        /// </summary>
        public void UnbindCancelInput()
        {
            if (_cancelAction != null)
            {
                _cancelAction.performed -= HandleCancelInputHandler;
            }
            _cancelAction = null;
            _inputModule = null;
        }

        /// <summary>
        ///     ポーズウィンドウを表示し、操作を受け付ける状態にする。
        /// </summary>
        public void Show()
        {
            _shownFrame = Time.frameCount;
            _isCancelPending = false;
            if (_windowRoot != null)
            {
                _windowRoot.gameObject.SetActive(true);
            }

            SetInteractionEnabled(true);

            // インゲーム中は隠しているカーソルを、マウス操作のために表示へ戻す。
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // 上下移動での選択を始められるよう、先頭のボタンへフォーカスを移す。
            SelectButton(_restartButton);
        }

        /// <summary>
        ///     ポーズウィンドウを非表示にし、インゲームの操作状態へ戻す。
        /// </summary>
        public void Hide()
        {
            SetInteractionEnabled(false);
            ClearSelection();

            if (_windowRoot != null)
            {
                _windowRoot.gameObject.SetActive(false);
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        /// <summary>
        ///     ボタンの操作可否を切り替える。
        ///     シーン遷移中に二重で押されるのを防ぐ用途で使用する。
        /// </summary>
        /// <param name="isEnabled"> 操作を受け付ける場合はtrue。 </param>
        public void SetInteractionEnabled(bool isEnabled)
        {
            _isInteractionEnabled = isEnabled;
            if (!isEnabled) { _isCancelPending = false; }
            if (_restartButton != null)
            {
                _restartButton.interactable = isEnabled;
            }

            if (_returnToTitleButton != null)
            {
                _returnToTitleButton.interactable = isEnabled;
            }
        }

        /// <summary>
        ///     シーン遷移に失敗した場合に、操作可能な状態へ復帰する。
        /// </summary>
        public void RestoreInteraction()
        {
            SetInteractionEnabled(true);
            SelectButton(_restartButton);
        }

        [SerializeField, Tooltip("表示と非表示を切り替えるウィンドウのルートオブジェクトです。")]
        private Image _windowRoot;

        [SerializeField, Tooltip("ステージをリスタートするボタンです。表示時に最初に選択されます。")]
        private Button _restartButton;

        [SerializeField, Tooltip("タイトルへ戻るボタンです。")]
        private Button _returnToTitleButton;

        private InputSystemUIInputModule _inputModule;
        private InputAction _cancelAction;
        private bool _isInteractionEnabled;
        private bool _isCancelPending;
        private int _shownFrame = -1;

        /// <summary> 再有効化時に取消操作の購読を復元します。 </summary>
        private void OnEnable()
        {
            if (_cancelAction != null)
            {
                _cancelAction.performed -= HandleCancelInputHandler;
                _cancelAction.performed += HandleCancelInputHandler;
            }
        }

        /// <summary> 無効化中は取消操作を受け付けません。 </summary>
        private void OnDisable()
        {
            _isCancelPending = false;
            if (_cancelAction != null)
            {
                _cancelAction.performed -= HandleCancelInputHandler;
            }
        }

        /// <summary>
        ///     表示中のポーズ画面だけを閉じ、別画面の取消操作を奪いません。
        /// </summary>
        /// <param name="context"> UI取消操作の通知です。 </param>
        private void HandleCancelInputHandler(InputAction.CallbackContext context)
        {
            if (_shownFrame != Time.frameCount && CanCancel())
            {
                _isCancelPending = true;
            }
        }

        /// <summary>
        ///     同じ入力に割り当てられた既存Option操作を先に処理し、まだ開いている時だけ取消を通知します。
        /// </summary>
        private void LateUpdate()
        {
            if (!_isCancelPending) { return; }
            _isCancelPending = false;
            if (CanCancel()) { OnCancelRequested?.Invoke(); }
        }

        /// <summary>
        ///     表示中・操作可能で、別画面に選択が移っていないことを確認します。
        /// </summary>
        /// <returns> このポーズ画面を取り消せる場合はtrue。 </returns>
        private bool CanCancel()
        {
            if (!_isInteractionEnabled || _windowRoot == null || !_windowRoot.gameObject.activeInHierarchy
                || _inputModule == null || !_inputModule.isActiveAndEnabled)
            {
                return false;
            }

            GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            return selected == null || selected == _restartButton?.gameObject || selected == _returnToTitleButton?.gameObject;
        }

        /// <summary>
        ///     ボタンのクリックを購読し、初期状態を非表示にする。
        /// </summary>
        private void Awake()
        {
            if (_windowRoot == null)
            {
                Debug.LogError($"[{nameof(PauseWindowView)}] ウィンドウのルートが設定されていません。", this);
                return;
            }

            if (_restartButton == null || _returnToTitleButton == null)
            {
                Debug.LogError($"[{nameof(PauseWindowView)}] ボタンの参照が設定されていません。", this);
                return;
            }

            _restartButton.onClick.AddListener(HandleRestartButtonClicked);
            _returnToTitleButton.onClick.AddListener(HandleReturnToTitleButtonClicked);

            SetInteractionEnabled(false);
            _windowRoot.gameObject.SetActive(false);
        }

        /// <summary>
        ///     購読したクリックを解除する。
        /// </summary>
        private void OnDestroy()
        {
            UnbindCancelInput();
            if (_restartButton != null)
            {
                _restartButton.onClick.RemoveListener(HandleRestartButtonClicked);
            }

            if (_returnToTitleButton != null)
            {
                _returnToTitleButton.onClick.RemoveListener(HandleReturnToTitleButtonClicked);
            }
        }

        /// <summary>
        ///     リスタートボタンのクリックを通知する。
        /// </summary>
        private void HandleRestartButtonClicked()
        {
            OnRestartRequested?.Invoke();
        }

        /// <summary>
        ///     タイトルへ戻るボタンのクリックを通知する。
        /// </summary>
        private void HandleReturnToTitleButtonClicked()
        {
            OnReturnToTitleRequested?.Invoke();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        /// <summary>
        ///     指定したボタンへEventSystemのフォーカスを移す。
        /// </summary>
        /// <param name="button"> フォーカス対象のボタン。 </param>
        private void SelectButton(Button button)
        {
            EventSystem eventSystem = EventSystem.current;

            if (eventSystem == null || button == null || !button.IsActive() || !button.IsInteractable())
            {
                return;
            }

            eventSystem.SetSelectedGameObject(null);
            eventSystem.SetSelectedGameObject(button.gameObject);
        }

        /// <summary>
        ///     ポーズウィンドウ内に残っているEventSystemの選択を解除する。
        /// </summary>
        private void ClearSelection()
        {
            EventSystem eventSystem = EventSystem.current;
            GameObject selectedObject = eventSystem != null
                ? eventSystem.currentSelectedGameObject
                : null;

            if (selectedObject == null || !selectedObject.transform.IsChildOf(transform))
            {
                return;
            }

            eventSystem.SetSelectedGameObject(null);
        }
    }
}
