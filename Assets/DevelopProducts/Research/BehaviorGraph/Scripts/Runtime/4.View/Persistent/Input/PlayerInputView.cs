using DevelopProducts.BehaviorGraph.Runtime.Adaptor.Persistent.Input;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace DevelopProducts.BehaviorGraph.Runtime.View.Persistent.Input
{
    /// <summary>
    ///     入力イベントを受け取り、InputContextを生成して外部に通知するクラス。
    /// </summary>
    [RequireComponent(typeof(PlayerInput), typeof(EventSystem), typeof(InputSystemUIInputModule))]
    public class PlayerInputView : MonoBehaviour
    {
        public void Initialize(InputTimestampProvider timestampProvider)
        {
            _timestampProvider = timestampProvider;
        }

        // イベント群。
        public event Action<InputContext<float>> OnOptionInput;

        public event Action<InputContext<float>> OnSubmitInput;
        public event Action<InputContext<float>> OnCancelInput;

        public event Action<InputContext<float>> OnDodgeInput;
        public event Action<InputContext<float>> OnAttackInput;
        public event Action<InputContext<Vector2>> OnMoveInput;
        public event Action<InputContext<Vector2>> OnLookInput;

        public void OnOption(InputAction.CallbackContext context)
        {
            float time = _timestampProvider.GetCurrentTimestamp();
            InputContext<float> inputContext = new InputContext<float>(
                InputActionKind.Option, context, time);
            OnOptionInput?.Invoke(inputContext);
        }

        public void OnSubmit(InputAction.CallbackContext context)
        {
            float time = _timestampProvider.GetCurrentTimestamp();
            InputContext<float> inputContext = new InputContext<float>(
                InputActionKind.Submit, context, time);
            OnSubmitInput?.Invoke(inputContext);
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            float time = _timestampProvider.GetCurrentTimestamp();
            InputContext<float> inputContext = new InputContext<float>(
                InputActionKind.Cancel, context, time);
            OnCancelInput?.Invoke(inputContext);
        }

        public void OnDodge(InputAction.CallbackContext context)
        {
            float time = _timestampProvider.GetCurrentTimestamp();
            InputContext<float> inputContext = new InputContext<float>(
                InputActionKind.Dodge, context, time);
            OnDodgeInput?.Invoke(inputContext);
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            float time = _timestampProvider.GetCurrentTimestamp();
            InputContext<float> inputContext = new InputContext<float>(
                InputActionKind.Attack, context, time);
            OnAttackInput?.Invoke(inputContext);
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            float time = _timestampProvider.GetCurrentTimestamp();
            InputContext<Vector2> inputContext = new InputContext<Vector2>(
                InputActionKind.Move, context, time);
            OnMoveInput?.Invoke(inputContext);
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            float time = _timestampProvider.GetCurrentTimestamp();
            InputContext<Vector2> inputContext = new InputContext<Vector2>(
                InputActionKind.Look, context, time);
            OnLookInput?.Invoke(inputContext);
        }

        public void OnMobileButton(InputActionKind actionId, InputActionPhase phase, float value)
        {
            Action<InputContext<float>> action = actionId switch
            {
                InputActionKind.Option => OnOptionInput,
                InputActionKind.Submit => OnSubmitInput,
                InputActionKind.Cancel => OnCancelInput,
                InputActionKind.Dodge => OnDodgeInput,
                InputActionKind.Attack => OnAttackInput,
                _ => null
            };

            if (action == null)
            {
                Debug.LogError($"Unsupported actionId: {actionId}");
                return;
            }

            float time = _timestampProvider.GetCurrentTimestamp();
            InputContext<float> inputContext = new InputContext<float>(
                actionId, value, phase, time);
            action?.Invoke(inputContext);
        }

        public void OnMobileMove(InputActionPhase phase, Vector2 value)
        {
            float time = _timestampProvider.GetCurrentTimestamp();
            InputContext<Vector2> inputContext = new InputContext<Vector2>(
                InputActionKind.Move, value, phase, time);
            OnMoveInput?.Invoke(inputContext);
        }

        public void OnMobileLook(InputActionPhase phase, Vector2 value)
        {
            float time = _timestampProvider.GetCurrentTimestamp();
            InputContext<Vector2> inputContext = new InputContext<Vector2>(
                InputActionKind.Look, value, phase, time);
            OnLookInput?.Invoke(inputContext);
        }

        private const string OPTION_ACTION_NAME = "Option";
        private const string SUBMIT_ACTION_NAME = "Submit";
        private const string CANCEL_ACTION_NAME = "Cancel";
        private const string DODGE_ACTION_NAME = "Dodge";
        private const string ATTACK_ACTION_NAME = "Attack";
        private const string MOVE_ACTION_NAME = "Move";
        private const string LOOK_ACTION_NAME = "Look";

        private PlayerInput _playerInput;
        private InputTimestampProvider _timestampProvider;

        private InputAction _optionAction;

        private InputAction _submitAction;
        private InputAction _cancelAction;

        private InputAction _dodgeAction;
        private InputAction _attackAction;
        private InputAction _moveAction;
        private InputAction _lookAction;

        private void Awake()
        {
            if (TryGetComponent(out _playerInput))
            {
                _playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
                if (_playerInput.uiInputModule == null)
                    { _playerInput.uiInputModule = GetComponent<InputSystemUIInputModule>(); }
            }
            else
            {
                Debug.LogError("PlayerInputコンポーネントが見つかりませんでした。");
                return;
            }

            CacheActions();
        }

        private void OnEnable()
        {
            RegistarAction(_optionAction, OnOption);
            RegistarAction(_submitAction, OnSubmit);
            RegistarAction(_cancelAction, OnCancel);
            RegistarAction(_dodgeAction, OnDodge);
            RegistarAction(_attackAction, OnAttack);
            RegistarAction(_moveAction, OnMove);
            RegistarAction(_lookAction, OnLook);
        }

        private void OnDisable()
        {
            UnregistarAction(_optionAction, OnOption);
            UnregistarAction(_submitAction, OnSubmit);
            UnregistarAction(_cancelAction, OnCancel);
            UnregistarAction(_dodgeAction, OnDodge);
            UnregistarAction(_attackAction, OnAttack);
            UnregistarAction(_moveAction, OnMove);
            UnregistarAction(_lookAction, OnLook);
        }

        /// <summary>
        ///     Actionをキャッシュして、後でイベントの登録と解除を行いやすくする。
        /// </summary>
        private void CacheActions()
        {
            InputActionAsset actions = _playerInput.actions;

            _optionAction = actions.FindAction($"{InputMapNames.Common}/{OPTION_ACTION_NAME}", true);
            _submitAction = actions.FindAction($"{InputMapNames.OutGame}/{SUBMIT_ACTION_NAME}", true);
            _cancelAction = actions.FindAction($"{InputMapNames.OutGame}/{CANCEL_ACTION_NAME}", true);
            _dodgeAction = actions.FindAction($"{InputMapNames.InGame}/{DODGE_ACTION_NAME}", true);
            _attackAction = actions.FindAction($"{InputMapNames.InGame}/{ATTACK_ACTION_NAME}", true);
            _moveAction = actions.FindAction($"{InputMapNames.InGame}/{MOVE_ACTION_NAME}", true);
            _lookAction = actions.FindAction($"{InputMapNames.InGame}/{LOOK_ACTION_NAME}", true);
        }

        /// <summary>
        ///     メソッドをInputactionに登録するためのヘルパーメソッド。
        /// </summary>
        /// <param name="action"></param>
        /// <param name="callback"></param>
        private static void RegistarAction(InputAction action, Action<InputAction.CallbackContext> callback)
        {
            action.started += callback;
            action.performed += callback;
            action.canceled += callback;
        }

        /// <summary>
        ///     メソッドをInputActionから解除するためのヘルパーメソッド。
        /// </summary>
        /// <param name="action"></param>
        /// <param name="callback"></param>
        private static void UnregistarAction(InputAction action, Action<InputAction.CallbackContext> callback)
        {
            action.started -= callback;
            action.performed -= callback;
            action.canceled -= callback;
        }
    }
}
