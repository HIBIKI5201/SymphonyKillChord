using UnityEngine;
using UnityEngine.InputSystem;

namespace Mock.CharacterControl
{
    [DefaultExecutionOrder(-1000)]
    [RequireComponent(typeof(PlayerInput))]
    public class InputBuffer : MonoBehaviour
    {
        public InputAction MoveAction => _playerInput.actions[_moveActionName];
        public InputAction RollAction => _playerInput.actions[_rollActionName];

        [SerializeField]
        private string _moveActionName = "Move";
        [SerializeField]
        private string _rollActionName = "Roll";

        private PlayerInput _playerInput;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
        }
    }
}
