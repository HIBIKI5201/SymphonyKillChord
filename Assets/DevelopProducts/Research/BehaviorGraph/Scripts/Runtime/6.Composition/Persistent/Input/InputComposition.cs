using DevelopProducts.BehaviorGraph.Runtime.Adaptor.Persistent.Input;
using DevelopProducts.BehaviorGraph.Runtime.Application.Persistent.Input;
using DevelopProducts.BehaviorGraph.Runtime.Domain.Persistent.Input;
using DevelopProducts.BehaviorGraph.Runtime.View.Persistent.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DevelopProducts.BehaviorGraph.Runtime.Composition
{
    /// <summary>
    ///     入力の初期化クラス。
    /// </summary>
    [RequireComponent(typeof(PlayerInputView), typeof(PlayerInput))]
    public class InputComposition : MonoBehaviour
    {
        public PlayerInputView GetInputView => _playerInputView;

        public UnityInputMapController GetInputMapController => _inputMapController;

        public InputBufferingQueue GetBufferedInputBuffer => _bufferedInputBuffer;


        [Header("Bufferの最大容量")]
        [SerializeField] private int _bufferCapacity;

        private PlayerInput _playerInput;
        private InputBufferingQueue _bufferedInputBuffer;
        private InputBufferRecorder _inputBufferRecorder;
        private RecordController _inputAdaptor;
        private PlayerInputView _playerInputView;
        private InputTimestampProvider _timestampProvider;
        private UnityInputMapController _inputMapController;

        private void Awake()
        {
            _playerInputView = GetComponent<PlayerInputView>();
            _playerInput = GetComponent<PlayerInput>();
            InitializePureObjects();
            InitializeInputMaps();
            BindViewToAdaptor();
        }

        private void OnDisable()
        {
            UnbindViewAdaptor();
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

            _inputMapController = new UnityInputMapController(commonMap, inGameMap, outGameMap);
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
            _playerInputView.OnLookInput += _inputAdaptor.HandleLook;
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
            _playerInputView.OnLookInput -= _inputAdaptor.HandleLook;
        }
    }
}
