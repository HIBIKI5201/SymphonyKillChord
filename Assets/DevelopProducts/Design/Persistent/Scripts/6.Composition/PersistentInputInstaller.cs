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
        public IInputBufferReader InputBufferReader => _inputStore;
        public IInputBufferWriter InputBufferWriter => _inputStore;
        public SwichInputMapUseCase SwichInputMapUseCase => _swichInputMapUseCase;

        [SerializeField] private PlayerInput _playerInput;

        [SerializeField] private PlayerInputView _playerInputView;
        [SerializeField] private InputBufferDebugView _inputDebugView;
        [SerializeField] private MobileInputButtonView[] _mobileInputButtonViews;

        [SerializeField] private int _bufferSize;
        [SerializeField] private bool _initializeToOutGame;

        private BufferdInputStore _inputStore;
        private InputTimestampProvider _timestampProvider;

        private BufferInputActionUsecase _bufferInputActionUsecase;
        private BufferButtonInputUsecase _bufferButtonInputUsecase;
        private SwichInputMapUseCase _swichInputMapUseCase;

        private void Install()
        {
            // Store / Providerの生成
            _inputStore = new BufferdInputStore(_bufferSize);
            _timestampProvider = new InputTimestampProvider();

            // Usecaseの生成
            _bufferButtonInputUsecase = new BufferButtonInputUsecase(_inputStore);
            _bufferInputActionUsecase = new BufferInputActionUsecase(_inputStore);

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
            _playerInputView.Initialize(_bufferButtonInputUsecase, 
                _bufferInputActionUsecase,
                _timestampProvider
                );

            if(_inputDebugView != null) 
                _inputDebugView.Initialize(_inputStore);

            if(_mobileInputButtonViews != null)
            {
                foreach (var mobileInputButtonView in _mobileInputButtonViews)
                {
                    mobileInputButtonView.Initialize(_bufferButtonInputUsecase, _timestampProvider);
                }
            }
        }

        private void Awake()
        {
            Install();
        }
    }
}