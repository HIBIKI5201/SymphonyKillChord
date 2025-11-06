using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mock.TPS
{
    /// <summary>
    ///     入力バッファクラス。
    /// </summary>
    [RequireComponent(typeof(PlayerInput)), DefaultExecutionOrder(-1000)]
    public class InputBuffer : MonoBehaviour
    {
        public InputActionEntity<Vector2> LookAction => _lookActionEntity;
        public InputActionEntity<Vector2> MoveAction => _moveActionEntity;

        [SerializeField]
        private string _lookActionName = "Look";
        [SerializeField]
        private string _moveActionName = "Move";

        private InputActionEntity<Vector2> _lookActionEntity;
        private InputActionEntity<Vector2> _moveActionEntity;

        public void Awake()
        {
            PlayerInput playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
                _lookActionEntity = new InputActionEntity<Vector2>(playerInput.actions[_lookActionName]);
                _moveActionEntity = new InputActionEntity<Vector2>(playerInput.actions[_moveActionName]);
            }
        }
    }

    public class InputActionEntity<T> where T : struct
    {
        public InputActionEntity(InputAction inputAction)
        {
            _inputAction = inputAction;
        }

        public event Action<T> Started
        {
            add
            {
                Action<InputAction.CallbackContext> handler = ctx => value(ctx.ReadValue<T>());
                _startedHandlers[value] = handler;
                _inputAction.started += handler;
            }
            remove
            {
                if (_startedHandlers.TryGetValue(value, out var handler))
                {
                    _inputAction.started -= handler;
                    _startedHandlers.Remove(value);
                }
            }
        }

        public event Action<T> Performed
        {
            add
            {
                Action<InputAction.CallbackContext> handler = ctx => value(ctx.ReadValue<T>());
                _performedHandlers[value] = handler;
                _inputAction.performed += handler;
            }
            remove
            {
                if (_performedHandlers.TryGetValue(value, out var handler))
                {
                    _inputAction.performed -= handler;
                    _performedHandlers.Remove(value);
                }
            }
        }

        public event Action<T> Canceled
        {
            add
            {
                Action<InputAction.CallbackContext> handler = ctx => value(ctx.ReadValue<T>());
                _canceledHandlers[value] = handler;
                _inputAction.canceled += handler;
            }
            remove
            {
                if (_canceledHandlers.TryGetValue(value, out var handler))
                {
                    _inputAction.canceled -= handler;
                    _canceledHandlers.Remove(value);
                }
            }
        }

        public void InvokeStarted(T value)
        {
            foreach (var kvp in _startedHandlers)
            {
                kvp.Key.Invoke(value);
            }
        }

        public void InvokePerfomed(T value)
        {
            foreach (var kvp in _performedHandlers)
            {
                kvp.Key.Invoke(value);
            }
        }

        public void InvokeCanceled(T value)
        {
            foreach (var kvp in _canceledHandlers)
            {
                kvp.Key.Invoke(value);
            }
        }

        private readonly InputAction _inputAction;

        // 各イベントごとにラムダを保持
        private readonly Dictionary<Action<T>, Action<InputAction.CallbackContext>> _startedHandlers = new();
        private readonly Dictionary<Action<T>, Action<InputAction.CallbackContext>> _performedHandlers = new();
        private readonly Dictionary<Action<T>, Action<InputAction.CallbackContext>> _canceledHandlers = new();
    }
}