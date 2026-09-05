using KillChord.Runtime.Adaptor.Persistent.Load;
using KillChord.Runtime.Adaptor.Persistent.Input;
using KillChord.Runtime.Application.Persistent.Input;
using KillChord.Runtime.Composition.Persistent.Bootstrap;
using KillChord.Runtime.Domain.Persistent.Input;
using KillChord.Runtime.View.Persistent.Input;
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

        public PlayerInputView GetInputView => _playerInputView;

        public UnityInputMapController GetInputMapController => _inputMapController;

        public InputBufferingQueue GetBufferedInputBuffer => _bufferedInputBuffer;


        [Header("Bufferの最大容量")]
        [SerializeField]
        private int _bufferCapacity;

        private PlayerInput _playerInput;
        private InputBufferingQueue _bufferedInputBuffer;
        private InputBufferRecorder _inputBufferRecorder;
        private RecordController _inputAdaptor;
        private PlayerInputView _playerInputView;
        private InputTimestampProvider _timestampProvider;
        private UnityInputMapController _inputMapController;
        private LoadingScreenController _loadingScreenController;
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
                || !ServiceLocator.TryGetInstance(out _loadingScreenController))
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

            ApplyInputSuppression(_loadingScreenController.IsLoading);
            return true;
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
            UnsubscribeLoading();
            UnbindViewAdaptor();

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
            ApplyInputSuppression(true);
        }

        /// <summary>
        ///     成否にかかわらず最終ロード終了時に入力を再開する。
        /// </summary>
        private void HandleLoadingCompleted(bool success)
        {
            ApplyInputSuppression(false);
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
