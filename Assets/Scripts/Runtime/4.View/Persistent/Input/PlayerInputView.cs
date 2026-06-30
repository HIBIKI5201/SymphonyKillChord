using KillChord.Runtime.Adaptor.Persistent.Input;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace KillChord.Runtime.View.Persistent.Input
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
            ServiceLocator.RegisterInstance(this);
        }

        // イベント群。
        public event Action<InputContext<float>> OnOptionInput;

        public event Action<InputContext<float>> OnSubmitInput;
        public event Action<InputContext<float>> OnCancelInput;

        public event Action<InputContext<float>> OnDodgeInput;
        public event Action<InputContext<float>> OnAttackInput;
        public event Action<InputContext<Vector2>> OnMoveInput;
        public event Action<InputContext<Vector2>> OnLookMouseInput;
        public event Action<InputContext<Vector2>> OnLookGamepadInput;
        /// <summary> ロックオン入力を通知するイベント。 </summary>
        public event Action<InputContext<float>> OnLockOnInput;
        public event Action<InputContext<Vector2>> OnMobileLookInput;

        public event Action<InputContext<float>> OnScenarioAdvanceInput;
        public event Action<InputContext<float>> OnScenarioFastForwardInput;
        public event Action<InputContext<float>> OnScenarioPauseInput;
        public event Action<InputContext<float>> OnScenarioSkipInput;
        public event Action<InputContext<float>> OnScenarioAutoInput;
        public event Action<InputContext<float>> OnScenarioHideUIInput;

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

            if (context.control?.device is Gamepad)
            {
                OnLookGamepadInput?.Invoke(inputContext);
            }
            else
            {
                OnLookMouseInput?.Invoke(inputContext);
            }
        }

        public void OnLockOn(InputAction.CallbackContext context)
        {
            float time = _timestampProvider.GetCurrentTimestamp();

            InputContext<float> inputContext = new InputContext<float>(
                InputActionKind.LockOn, context, time);
            OnLockOnInput?.Invoke(inputContext);
        }

        public void OnScenarioAdvance(InputAction.CallbackContext context)
        {
            float time = _timestampProvider.GetCurrentTimestamp();
            InputContext<float> inputContext = new InputContext<float>(
                InputActionKind.ScenarioAdvance, context, time);
            OnScenarioAdvanceInput?.Invoke(inputContext);
        }

        public void OnScenarioFastForward(InputAction.CallbackContext context)
        {
            float time = _timestampProvider.GetCurrentTimestamp();
            InputContext<float> inputContext = new InputContext<float>(
                InputActionKind.ScenarioFastForward, context, time);
            OnScenarioFastForwardInput?.Invoke(inputContext);
        }

        public void OnScenarioPause(InputAction.CallbackContext context)
        {
            float time = _timestampProvider.GetCurrentTimestamp();
            InputContext<float> inputContext = new InputContext<float>(
                InputActionKind.ScenarioPause, context, time);
            OnScenarioPauseInput?.Invoke(inputContext);
        }

        public void OnScenarioSkip(InputAction.CallbackContext context)
        {
            float time = _timestampProvider.GetCurrentTimestamp();
            InputContext<float> inputContext = new InputContext<float>(
                InputActionKind.ScenarioSkip, context, time);
            OnScenarioSkipInput?.Invoke(inputContext);
        }

        public void OnScenarioAuto(InputAction.CallbackContext context)
        {
            float time = _timestampProvider.GetCurrentTimestamp();
            InputContext<float> inputContext = new InputContext<float>(
                InputActionKind.ScenarioAuto, context, time);
            OnScenarioAutoInput?.Invoke(inputContext);
        }

        public void OnScenarioHideUI(InputAction.CallbackContext context)
        {
            float time = _timestampProvider.GetCurrentTimestamp();
            InputContext<float> inputContext = new InputContext<float>(
                InputActionKind.ScenarioHideUI, context, time);
            OnScenarioHideUIInput?.Invoke(inputContext);
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
            OnMobileLookInput?.Invoke(inputContext);
        }

        private const string OPTION_ACTION_NAME = "Option";
        private const string SUBMIT_ACTION_NAME = "Submit";
        private const string CANCEL_ACTION_NAME = "Cancel";
        private const string DODGE_ACTION_NAME = "Dodge";
        private const string ATTACK_ACTION_NAME = "Attack";
        private const string MOVE_ACTION_NAME = "Move";
        private const string LOOK_ACTION_NAME = "Look";
        private const string LOCK_ON_ACTION_NAME = "LockOn";
        private const string SCENARIO_ADVANCE_ACTION_NAME = "Advance";
        private const string SCENARIO_FAST_FORWARD_ACTION_NAME = "FastForward";
        private const string SCENARIO_PAUSE_ACTION_NAME = "Pause";
        private const string SCENARIO_SKIP_ACTION_NAME = "Skip";
        private const string SCENARIO_AUTO_ACTION_NAME = "Auto";
        private const string SCENARIO_HIDE_UI_ACTION_NAME = "HideUI";

        private PlayerInput _playerInput;
        private InputTimestampProvider _timestampProvider;

        private InputAction _optionAction;

        private InputAction _submitAction;
        private InputAction _cancelAction;

        private InputAction _dodgeAction;
        private InputAction _attackAction;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _lockOnAction;

        private InputAction _scenarioAdvanceAction;
        private InputAction _scenarioFastForwardAction;
        private InputAction _scenarioPauseAction;
        private InputAction _scenarioSkipAction;
        private InputAction _scenarioAutoAction;
        private InputAction _scenarioHideUIAction;

        private void Awake()
        {
            if (TryGetComponent(out _playerInput))
            {
                _playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
                if (_playerInput.uiInputModule == null)
                {
                    _playerInput.uiInputModule = GetComponent<InputSystemUIInputModule>();
                }
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
            RegisterAction(_optionAction, OnOption);
            RegisterAction(_submitAction, OnSubmit);
            RegisterAction(_cancelAction, OnCancel);
            RegisterAction(_dodgeAction, OnDodge);
            RegisterAction(_attackAction, OnAttack);
            RegisterAction(_moveAction, OnMove);
            RegisterAction(_lookAction, OnLook);
            RegisterAction(_lockOnAction, OnLockOn);
            RegisterAction(_scenarioAdvanceAction, OnScenarioAdvance);
            RegisterAction(_scenarioFastForwardAction, OnScenarioFastForward);
            RegisterAction(_scenarioPauseAction, OnScenarioPause);
            RegisterAction(_scenarioSkipAction, OnScenarioSkip);
            RegisterAction(_scenarioAutoAction, OnScenarioAuto);
            RegisterAction(_scenarioHideUIAction, OnScenarioHideUI);
        }

        private void OnDisable()
        {
            UnregisterAction(_optionAction, OnOption);
            UnregisterAction(_submitAction, OnSubmit);
            UnregisterAction(_cancelAction, OnCancel);
            UnregisterAction(_dodgeAction, OnDodge);
            UnregisterAction(_attackAction, OnAttack);
            UnregisterAction(_moveAction, OnMove);
            UnregisterAction(_lookAction, OnLook);
            UnregisterAction(_lockOnAction, OnLockOn);
            UnregisterAction(_scenarioAdvanceAction, OnScenarioAdvance);
            UnregisterAction(_scenarioFastForwardAction, OnScenarioFastForward);
            UnregisterAction(_scenarioPauseAction, OnScenarioPause);
            UnregisterAction(_scenarioSkipAction, OnScenarioSkip);
            UnregisterAction(_scenarioAutoAction, OnScenarioAuto);
            UnregisterAction(_scenarioHideUIAction, OnScenarioHideUI);
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
            _lockOnAction = actions.FindAction($"{InputMapNames.InGame}/{LOCK_ON_ACTION_NAME}", true);
            _scenarioAdvanceAction = actions.FindAction($"{InputMapNames.Scenario}/{SCENARIO_ADVANCE_ACTION_NAME}", true);
            _scenarioFastForwardAction = actions.FindAction($"{InputMapNames.Scenario}/{SCENARIO_FAST_FORWARD_ACTION_NAME}", true);
            _scenarioPauseAction = actions.FindAction($"{InputMapNames.Scenario}/{SCENARIO_PAUSE_ACTION_NAME}", true);
            _scenarioSkipAction = actions.FindAction($"{InputMapNames.Scenario}/{SCENARIO_SKIP_ACTION_NAME}", true);
            _scenarioAutoAction = actions.FindAction($"{InputMapNames.Scenario}/{SCENARIO_AUTO_ACTION_NAME}", true);
            _scenarioHideUIAction = actions.FindAction($"{InputMapNames.Scenario}/{SCENARIO_HIDE_UI_ACTION_NAME}", true);
        }

        /// <summary>
        ///     メソッドをInputactionに登録するためのヘルパーメソッド。
        /// </summary>
        /// <param name="action"></param>
        /// <param name="callback"></param>
        private static void RegisterAction(InputAction action, Action<InputAction.CallbackContext> callback)
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
        private static void UnregisterAction(InputAction action, Action<InputAction.CallbackContext> callback)
        {
            action.started -= callback;
            action.performed -= callback;
            action.canceled -= callback;
        }
    }
}