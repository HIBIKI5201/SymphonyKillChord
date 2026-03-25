using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Application;
using KillChord.Runtime.View;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KillChord.Runtime.Composition
{
    /// <summary>
    ///     入力の初期化クラス。
    /// </summary>
    public class InputComposition : MonoBehaviour
    {
        public PlayerInputView GetInputView => _playerInputView;

        public UnityInputMapController GetInputMapController => _inputMapController;

        public BufferedInputBuffer GetBufferedInputBuffer => _bufferedInputBuffer;

        [Header("PlayerInput")]
        [SerializeField] private PlayerInput _playerInput;

        [Header("Bufferの最大容量")]
        [SerializeField] private int _bufferCapacity;

        private BufferedInputBuffer _bufferedInputBuffer;
        private InputBufferRecorder _inputBufferRecorder;
        private InputAdaptor _inputAdaptor;
        private PlayerInputView _playerInputView;
        private InputTimestampProvider _timestampProvider;
        private UnityInputMapController _inputMapController;

        private InputAction _optionAction;
        private InputAction _submitAction;
        private InputAction _cancelAction;
        private InputAction _moveAction;
        private InputAction _dodgeAction;
        private InputAction _attackAction;

        /// <summary>
        ///     クラスの初期化を行う。
        /// </summary>
        private void InitializePureObjects()
        {
            _bufferedInputBuffer = new BufferedInputBuffer(_bufferCapacity);
            _inputBufferRecorder = new InputBufferRecorder(_bufferedInputBuffer);
            _inputAdaptor = new InputAdaptor(_inputBufferRecorder);

            _timestampProvider = new InputTimestampProvider();
            _playerInputView = new PlayerInputView(_timestampProvider);
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
        ///     Actionをキャッシュして、後でイベントの登録と解除を行いやすくする。
        /// </summary>
        private void CacheActions()
        {
            InputActionAsset actions = _playerInput.actions;

            _optionAction = actions.FindAction($"{InputMapNames.Common}/Option", true);
            _submitAction = actions.FindAction($"{InputMapNames.OutGame}/Submit", true);
            _cancelAction = actions.FindAction($"{InputMapNames.OutGame}/Cancel", true);
            _moveAction = actions.FindAction($"{InputMapNames.InGame}/Move", true);
            _dodgeAction = actions.FindAction($"{InputMapNames.InGame}/Dodge", true);
            _attackAction = actions.FindAction($"{InputMapNames.InGame}/Attack", true);
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
        }

        /// <summary>
        ///     InputActionのイベントにViewの処理を登録する。
        /// </summary>
        private void RegisterActions()
        {
            _optionAction.started += _playerInputView.OnOption;
            _optionAction.performed += _playerInputView.OnOption;
            _optionAction.canceled += _playerInputView.OnOption;

            _submitAction.started += _playerInputView.OnSubmit;
            _submitAction.performed += _playerInputView.OnSubmit;
            _submitAction.canceled += _playerInputView.OnSubmit;

            _cancelAction.started += _playerInputView.OnCancel;
            _cancelAction.performed += _playerInputView.OnCancel;
            _cancelAction.canceled += _playerInputView.OnCancel;

            _moveAction.started += _playerInputView.OnMove;
            _moveAction.performed += _playerInputView.OnMove;
            _moveAction.canceled += _playerInputView.OnMove;

            _dodgeAction.started += _playerInputView.OnDodge;
            _dodgeAction.performed += _playerInputView.OnDodge;
            _dodgeAction.canceled += _playerInputView.OnDodge;

            _attackAction.started += _playerInputView.OnAttack;
            _attackAction.performed += _playerInputView.OnAttack;
            _attackAction.canceled += _playerInputView.OnAttack;
        }

        /// <summary>
        ///     InputActionのイベントからViewの処理を解除する。
        /// </summary>
        private void UnregisterActions()
        {
            _optionAction.started -= _playerInputView.OnOption;
            _optionAction.performed -= _playerInputView.OnOption;
            _optionAction.canceled -= _playerInputView.OnOption;

            _submitAction.started -= _playerInputView.OnSubmit;
            _submitAction.performed -= _playerInputView.OnSubmit;
            _submitAction.canceled -= _playerInputView.OnSubmit;

            _cancelAction.started -= _playerInputView.OnCancel;
            _cancelAction.performed -= _playerInputView.OnCancel;
            _cancelAction.canceled -= _playerInputView.OnCancel;

            _moveAction.started -= _playerInputView.OnMove;
            _moveAction.performed -= _playerInputView.OnMove;
            _moveAction.canceled -= _playerInputView.OnMove;

            _dodgeAction.started -= _playerInputView.OnDodge;
            _dodgeAction.performed -= _playerInputView.OnDodge;
            _dodgeAction.canceled -= _playerInputView.OnDodge;

            _attackAction.started -= _playerInputView.OnAttack;
            _attackAction.performed -= _playerInputView.OnAttack;
            _attackAction.canceled -= _playerInputView.OnAttack;
        }

        private void Awake()
        {
            InitializePureObjects();
            InitializeInputMaps();
            CacheActions();
            BindViewToAdaptor();
        }

        private void OnEnable()
        {
            RegisterActions();
        }

        private void OnDisable()
        {
            UnregisterActions();
        }
    }
}
