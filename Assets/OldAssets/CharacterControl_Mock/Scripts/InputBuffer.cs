using UnityEngine;
using UnityEngine.InputSystem;

namespace Mock.CharacterControl
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputBuffer : MonoBehaviour
    {
        public InputAction MoveAction => _playerInput.actions[_moveActionName];

        [SerializeField]
        private string _moveActionName = "Move";

        private PlayerInput _playerInput;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
        }
    }
}
