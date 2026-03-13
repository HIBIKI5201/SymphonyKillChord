using DevelopProducts.Persistent.Adaptor;
using DevelopProducts.Persistent.Application;
using DevelopProducts.Persistent.Domain.Input;
using DevelopProducts.Persistent.View;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DevelopProducts.Persistent.Composition
{
    /// <summary>
    ///     PersistentInput関連のクラスをインストールするクラス。
    ///     PersistentInput関連のクラスを生成して、必要な依存関係を注入する。
    /// </summary>
    public class PersistentInputInstaller : MonoBehaviour
    {
        public BufferdInputBuffer BufferedInputBuffer => _bufferedInputBuffer;
        public SwichInputMapUseCase SwichInputMapUseCase => _swichInputMapUseCase;
        public BufferButtonInputUsecase BufferButtonInputUsecase => _bufferButtonInputUsecase;
        public BufferMoveInputUsecase BufferMoveInputUsecase => _bufferInputActionUsecase;
        public InputTimestampProvider TimestampProvider => _timestampProvider;

        [SerializeField] private PlayerInput _playerInput;

        [SerializeField] private PlayerInputView _playerInputView;
        [SerializeField] private InputBufferDebugView _inputDebugView;
        [SerializeField] private MobileInputButtonView[] _mobileInputButtonViews;

        [SerializeField] private int _bufferSize;
        [SerializeField] private bool _initializeToOutGame;

        private BufferdInputBuffer _bufferedInputBuffer;
        private InputTimestampProvider _timestampProvider;

        private BufferMoveInputUsecase _bufferInputActionUsecase;
        private BufferButtonInputUsecase _bufferButtonInputUsecase;
        private SwichInputMapUseCase _swichInputMapUseCase;
        private ButtonInputAdaptor _buttonInputAdaptor;
        private MoveInputAdaptor _moveInputAdaptor;

        private void Install()
        {
            // Store / Providerの生成
            _bufferedInputBuffer = new BufferdInputBuffer(_bufferSize);
            _timestampProvider = new InputTimestampProvider();

            // Usecaseの生成
            _bufferButtonInputUsecase = new BufferButtonInputUsecase(_bufferedInputBuffer);
            _bufferInputActionUsecase = new BufferMoveInputUsecase(_bufferedInputBuffer);

            // Adaptorの生成
            _moveInputAdaptor = new MoveInputAdaptor(_bufferInputActionUsecase, _timestampProvider);
            _buttonInputAdaptor = new ButtonInputAdaptor(_bufferButtonInputUsecase, _timestampProvider);

            // ActionMapの取得
            InputActionMap commonMap = _playerInput.actions.FindActionMap(InputMapNames.Common);
            InputActionMap inGameMap = _playerInput.actions.FindActionMap(InputMapNames.InGame);
            InputActionMap outGameMap = _playerInput.actions.FindActionMap(InputMapNames.OutGame);

            IInputMapController inputMapController = new UnityInputMapController(
                commonMap,
                inGameMap,
                outGameMap
                );

            _swichInputMapUseCase = new SwichInputMapUseCase(inputMapController);

            // Viewに依存性注入
            _playerInputView.Initialize(_buttonInputAdaptor
                , _moveInputAdaptor
                );

            if(_inputDebugView != null) 
                _inputDebugView.Initialize(_bufferedInputBuffer);

            if(_mobileInputButtonViews != null)
            {
                foreach (var mobileInputButtonView in _mobileInputButtonViews)
                {
                    if(mobileInputButtonView == null) continue;
                    mobileInputButtonView.Initialize(_buttonInputAdaptor);
                }
            }

            if (_initializeToOutGame)
            {
                _swichInputMapUseCase.ToOutGame();
            }
            else
            {
                _swichInputMapUseCase.DisableAll();
            }
        }

        private void Awake()
        {
            Install();
        }
    }
}