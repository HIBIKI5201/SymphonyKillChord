using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mock.TPS
{
    /// <summary>
    ///     入力バッファクラス。
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
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

        public void Start()
        {
            PlayerInput playerInput = GetComponent<PlayerInput>();
            _lookActionEntity = new InputActionEntity<Vector2>(playerInput.actions[_lookActionName]);
            _moveActionEntity = new InputActionEntity<Vector2>(playerInput.actions[_moveActionName]);
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
            add => _inputAction.started += ctx => value(ctx.ReadValue<T>());
            remove => _inputAction.started -= ctx => value(ctx.ReadValue<T>());
        }

        public event Action<T> Performed
        {
            add => _inputAction.performed += ctx => value(ctx.ReadValue<T>());
            remove => _inputAction.performed -= ctx => value(ctx.ReadValue<T>());
        }

        public event Action<T> Canceled
        {
            add => _inputAction.canceled += ctx => value(ctx.ReadValue<T>());
            remove => _inputAction.canceled -= ctx => value(ctx.ReadValue<T>());
        }

        private InputAction _inputAction;
    }
}