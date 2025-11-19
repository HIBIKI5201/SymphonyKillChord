using Mock.MusicBattle.Develop;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mock.MusicBattle
{
    [Obsolete("これは仮実装クラスです。")]
    [RequireComponent(typeof(PlayerInput))]
    public class CameraInputBuffer : MonoBehaviour, IInputBuffer
    {
        public event Action<Vector2> LookAction;
        public event Action<float> LockOnSelectAction;

        [SerializeField]
        private string _lookActionName = "Look";
        [SerializeField]
        private string _lookOnSelectName = "LockOnSelect";

        private void Awake()
        {
            PlayerInput input = GetComponent<PlayerInput>();
            if (input == null) { return; }

            InputAction look = input.actions[_lookActionName];
            InputAction lockOnSelect = input.actions[_lookOnSelectName];

            look.started += InvokeLookAction;
            look.performed += InvokeLookAction;
            look.canceled += InvokeLookAction;

            lockOnSelect.started += InvokeLockOnSelect;
            lockOnSelect.performed += InvokeLockOnSelect;
            lockOnSelect.canceled += InvokeLockOnSelect;
        }

        private void InvokeLookAction(InputAction.CallbackContext context)
        {
            // カメラはperformedとcanceledの時だけ実行。
            if (context.phase == InputActionPhase.Started) { return; }
            Vector2 value = context.ReadValue<Vector2>();
            LookAction?.Invoke(value);
        }

        private void InvokeLockOnSelect(InputAction.CallbackContext context)
        {
            // カメラはstartedの時だけ実行。
            if (context.phase != InputActionPhase.Started) { return; }
            Vector2 value = context.ReadValue<Vector2>();
            LookAction?.Invoke(value);
        }
    }
}
