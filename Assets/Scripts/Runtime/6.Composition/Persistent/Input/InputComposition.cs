using KillChord.Runtime.Adaptor.Persistent.Environment;
using KillChord.Runtime.Adaptor.Persistent.Load;
using KillChord.Runtime.Adaptor.Persistent.Input;
using KillChord.Runtime.Application.Persistent.Input;
using KillChord.Runtime.Composition.Persistent.Bootstrap;
using KillChord.Runtime.Composition.Persistent.Environment;
using KillChord.Runtime.Domain.Persistent.Input;
using KillChord.Runtime.View.Persistent.Input;
using KillChord.Runtime.View.Persistent.Load;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KillChord.Runtime.Composition.Persistent.Input
{
    /// <summary>
    ///     入力の初期化クラス。
    /// </summary>
    [RequireComponent(typeof(PlayerInputView), typeof(PlayerInput))]
    public sealed class InputComposition : PersistentInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(InputComposition);

        /// <summary> 実行順です。 </summary>
        public override int Order => 50;

        /// <summary> プレイヤーの入力を受け取るビュー。 </summary>
        public PlayerInputView GetInputView => _playerInputView;

        /// <summary> 入力マップの切り替えを制御するコントローラー。 </summary>
        public UnityInputMapController GetInputMapController => _inputMapController;

        /// <summary> 入力を記録するバッファ。 </summary>
        public InputBufferingQueue GetBufferedInputBuffer => _bufferedInputBuffer;


        [Header("Bufferの最大容量")]
        [SerializeField, Tooltip("入力バッファに保持する入力の数。")]
        private int _bufferCapacity;

        private PlayerInput _playerInput;
        private InputBufferingQueue _bufferedInputBuffer;
        private InputBufferRecorder _inputBufferRecorder;
        private RecordController _inputAdaptor;
        private PlayerInputView _playerInputView;
        private InputTimestampProvider _timestampProvider;
        private UnityInputMapController _inputMapController;
        private GamepadButtonLayoutView _gamepadButtonLayoutView;
        private LoadingScreenController _loadingScreenController;
        private EventNotificationView _notificationView;
        private bool _isNotificationSubscribed;
        private bool _isLoadingSubscribed;
        private bool _isViewBound;

        /// <summary>
        ///     入力関連の純粋オブジェクトとView連携を構築する。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            _playerInputView = GetComponent<PlayerInputView>();
            _playerInput = GetComponent<PlayerInput>();
            InitializePureObjects();
            InitializeInputMaps();
            BindViewToAdaptor();
            ServiceLocator.RegisterInstance(_playerInputView);
            ServiceLocator.RegisterInstance(this, LocateTypeEnum.Locator);
            return true;
        }

        /// <summary>
        ///     ロードセッションを購読してから現在の入力抑止状態を同期する。
        /// </summary>
        public override bool Ready()
        {
            if (_playerInputView == null || !_playerInputView.HasUIInputModule
                || _inputMapController == null
                || !ServiceLocator.TryGetInstance(out _loadingScreenController)
                || !ServiceLocator.TryGetInstance(out _notificationView))
            {
                Debug.LogError($"[{nameof(InputComposition)}] ロード中の入力制御に必要な依存を取得できませんでした。", this);
                return false;
            }

            if (!_isLoadingSubscribed)
            {
                _loadingScreenController.LoadingStarted += HandleLoadingStarted;
                _loadingScreenController.LoadingCompleted += HandleLoadingCompleted;
                _isLoadingSubscribed = true;
            }

            if (!_isNotificationSubscribed)
            {
                _notificationView.OnVisibilityChanged += HandleNotificationVisibilityChanged;
                _isNotificationSubscribed = true;
            }

            RefreshInputSuppression();
            BindGamepadButtonLayout();
            return true;
        }

        /// <summary>
        ///     ゲームパッドの決定・キャンセルの配置を環境設定に合わせる。
        ///     環境設定を取得できない場合は既定の海外式で固定する。
        /// </summary>
        private void BindGamepadButtonLayout()
        {
            if (_gamepadButtonLayoutView != null)
            {
                return;
            }

            IEnvironmentSettingsViewModel environmentSettingsViewModel =
                ServiceLocator.TryGetInstance(out EnvironmentSettingsModuleContainer environmentSettingsContainer)
                    ? environmentSettingsContainer.ViewModel
                    : null;
            if (environmentSettingsViewModel == null)
            {
                Debug.LogWarning(
                    $"[{nameof(InputComposition)}] 環境設定を取得できないため、決定・キャンセルは既定の配置で続行します。",
                    this);
            }

            _gamepadButtonLayoutView = new GamepadButtonLayoutView(_playerInput.actions, environmentSettingsViewModel);
        }

        /// <summary>
        ///     無効化時に入力記録の購読を解除する。
        /// </summary>
        private void OnDisable()
        {
            UnbindViewAdaptor();
        }

        /// <summary>
        ///     登録済み入力サービスを解除する。
        /// </summary>
        public override void Shutdown()
        {
            if (_isNotificationSubscribed && _notificationView != null)
            {
                _notificationView.OnVisibilityChanged -= HandleNotificationVisibilityChanged;
            }
            _isNotificationSubscribed = false;
            _notificationView = null;
            UnsubscribeLoading();
            UnbindViewAdaptor();
            _gamepadButtonLayoutView?.Dispose();
            _gamepadButtonLayoutView = null;

            if (ServiceLocator.TryGetInstance(out PlayerInputView registeredInputView)
                && ReferenceEquals(registeredInputView, _playerInputView))
            {
                ServiceLocator.UnregisterInstance<PlayerInputView>();
            }

            if (ServiceLocator.TryGetInstance(out InputComposition registeredInputComposition)
                && ReferenceEquals(registeredInputComposition, this))
            {
                ServiceLocator.UnregisterInstance(this);
            }
        }

        /// <summary>
        ///     破棄時の安全側解除を行う。
        /// </summary>
        private void OnDestroy()
        {
            Shutdown();
        }

        /// <summary>
        ///     ロード開始時に入力通知と全マップを抑止する。
        /// </summary>
        private void HandleLoadingStarted()
        {
            RefreshInputSuppression();
        }

        /// <summary>
        ///     成否にかかわらず最終ロード終了時に入力を再開する。
        /// </summary>
        private void HandleLoadingCompleted(bool success)
        {
            RefreshInputSuppression();
        }

        /// <summary>
        ///     通知表示の変更時にロード状態と入力抑止を合成します。
        /// </summary>
        private void HandleNotificationVisibilityChanged(bool isVisible)
        {
            RefreshInputSuppression();
        }

        /// <summary>
        ///     一方の完了で他方の入力抑止を解除しないよう状態を同期します。
        /// </summary>
        private void RefreshInputSuppression()
        {
            ApplyInputSuppression((_loadingScreenController != null && _loadingScreenController.IsLoading)
                || (_notificationView != null && _notificationView.IsVisible));
        }

        /// <summary>
        ///     canceled通知とUIモジュール再有効化中の通知が漏れない順序で入力状態を適用する。
        /// </summary>
        private void ApplyInputSuppression(bool isSuppressed)
        {
            try
            {
                if (isSuppressed)
                {
                    _playerInputView.SetInputEnabled(false);
                    _inputMapController.SetInputSuppressed(true);
                }
                else
                {
                    _inputMapController.SetInputSuppressed(false);
                    _playerInputView.SetInputEnabled(true);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        /// <summary>
        ///     購読済みのロード通知を一度だけ解除する。
        /// </summary>
        private void UnsubscribeLoading()
        {
            if (_isLoadingSubscribed && _loadingScreenController != null)
            {
                _loadingScreenController.LoadingStarted -= HandleLoadingStarted;
                _loadingScreenController.LoadingCompleted -= HandleLoadingCompleted;
            }

            _isLoadingSubscribed = false;
            _loadingScreenController = null;
        }

        /// <summary>
        ///     クラスの初期化を行う。
        /// </summary>
        private void InitializePureObjects()
        {
            _bufferedInputBuffer = new InputBufferingQueue(_bufferCapacity);
            _inputBufferRecorder = new InputBufferRecorder(_bufferedInputBuffer);
            _inputAdaptor = new RecordController(_inputBufferRecorder);

            _timestampProvider = new InputTimestampProvider();
            _playerInputView.Initialize(_timestampProvider);
        }

        /// <summary>
        ///     InputActionMapをUnityInputMapControllerに渡して初期化する。
        /// </summary>
        private void InitializeInputMaps()
        {
            InputActionAsset actions = _playerInput.actions;

            InputActionMap commonMap = actions.FindActionMap(InputMapNames.Common, true);
            InputActionMap inGameMap = actions.FindActionMap(InputMapNames.InGame, true);
            InputActionMap outGameMap = actions.FindActionMap(InputMapNames.OutGame, true);
            InputActionMap scenarioMap = actions.FindActionMap(InputMapNames.Scenario, true);

            InputActionMap uiMap = actions.FindActionMap(InputMapNames.UI, true);

            _inputMapController = new UnityInputMapController(commonMap, inGameMap, outGameMap, scenarioMap, uiMap);
        }

        /// <summary>
        ///     ViewのイベントにAdaptorの処理を登録する。
        /// </summary>
        private void BindViewToAdaptor()
        {
            if (_isViewBound)
            {
                return;
            }

            _isViewBound = true;
            _playerInputView.OnOptionInput += _inputAdaptor.HandleButton;
            _playerInputView.OnSubmitInput += _inputAdaptor.HandleButton;
            _playerInputView.OnCancelInput += _inputAdaptor.HandleButton;
            _playerInputView.OnDodgeInput += _inputAdaptor.HandleButton;
            _playerInputView.OnAttackInput += _inputAdaptor.HandleButton;
            _playerInputView.OnMoveInput += _inputAdaptor.HandleMove;
            _playerInputView.OnLookMouseInput += _inputAdaptor.HandleLook;
        }

        /// <summary>
        ///     ViewのイベントからAdaptorの処理を解除する。
        /// </summary>
        private void UnbindViewAdaptor()
        {
            if (!_isViewBound || _playerInputView == null || _inputAdaptor == null)
            {
                return;
            }

            _isViewBound = false;
            _playerInputView.OnOptionInput -= _inputAdaptor.HandleButton;
            _playerInputView.OnSubmitInput -= _inputAdaptor.HandleButton;
            _playerInputView.OnCancelInput -= _inputAdaptor.HandleButton;
            _playerInputView.OnDodgeInput -= _inputAdaptor.HandleButton;
            _playerInputView.OnAttackInput -= _inputAdaptor.HandleButton;
            _playerInputView.OnMoveInput -= _inputAdaptor.HandleMove;
            _playerInputView.OnLookMouseInput -= _inputAdaptor.HandleLook;
        }
    }
}
