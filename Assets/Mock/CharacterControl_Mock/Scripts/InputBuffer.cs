using UnityEngine;
using UnityEngine.InputSystem;

namespace Mock.CharacterControl
{
    [DefaultExecutionOrder(-1000)]
    [RequireComponent(typeof(PlayerInput))]
    public class InputBuffer : MonoBehaviour
    {
        public InputAction MoveAction => _playerInput.actions[_moveActionName];
        public InputAction AttackAction => _playerInput.actions[_attackActionName];

        [SerializeField]
        private string _moveActionName = "Move";
        [SerializeField]
        private string _attackActionName = "Attack";

        private PlayerInput _playerInput;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
        }
    }
}
