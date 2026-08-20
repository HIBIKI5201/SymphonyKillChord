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
            ServiceLocator.RegisterInstance(this, LocateType.Locator);
            return true;
        }

        private void OnDisable()
        {
            UnbindViewAdaptor();
        }

        /// <summary>
        ///     登録済み入力サービスを解除する。
        /// </summary>
        public override void Shutdown()
        {
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

            _inputMapController = new UnityInputMapController(commonMap, inGameMap, outGameMap, scenarioMap);
        }

        /// <summary>
        ///     ViewのイベントにAdaptorの処理を登録する。
        /// </summary>
        private void BindViewToAdaptor()
        {
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
