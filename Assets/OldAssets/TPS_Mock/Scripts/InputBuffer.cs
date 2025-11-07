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
        public InputActionEntity<float> JumpAction => _jumpActionEntity;
        public InputActionEntity<float> AttackAction => _attackActionEntity;

        [SerializeField]
        private string _lookActionName = "Look";
        [SerializeField]
        private string _moveActionName = "Move";
        [SerializeField]
        private string _jumpActionName = "Jump";
        [SerializeField]
        private string _attackActionName = "Fire";

        private InputActionEntity<Vector2> _lookActionEntity;
        private InputActionEntity<Vector2> _moveActionEntity;
        private InputActionEntity<float> _jumpActionEntity;
        private InputActionEntity<float> _attackActionEntity;

        public void Awake()
        {
            PlayerInput playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
                _lookActionEntity = new InputActionEntity<Vector2>(playerInput.actions[_lookActionName]);
                _moveActionEntity = new InputActionEntity<Vector2>(playerInput.actions[_moveActionName]);
                _attackActionEntity = new InputActionEntity<float>(playerInput.actions[_jumpActionName]);
                _attackActionEntity = new InputActionEntity<float>(playerInput.actions[_attackActionName]);
            }
        }
    }
}