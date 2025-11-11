using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace Mock.CharacterControl
{
    /// <summary>
    ///     プレイヤーの管理クラス。
    /// </summary>
    public class PlayerManager
    {
        public PlayerManager(PlayerStatus status,
            SymphonyAnimeController animeController)
        {
            _status = status;

            if (animeController == null) { return; }

            _animeController = animeController;
            if (animeController.TryGetComponent(out _rigidbody)) 
            {
                _rigidbody.isKinematic = true;
            }
            else
            {
                Debug.LogError($"{animeController.name}に{nameof(Rigidbody)}がありません。");
            }


            if (!animeController.TryGetComponent(out _agent)) { Debug.LogError($"{animeController.name}に{nameof(NavMeshAgent)}がありません。"); }
        }

        public void InputRegister(InputBuffer inputBuffer, CancellationToken token = default)
        {
            inputBuffer.MoveAction.performed += HandleMove;
            inputBuffer.MoveAction.canceled += HandleMove;

            token.Register(() =>
            {
                inputBuffer.MoveAction.performed -= HandleMove;
                inputBuffer.MoveAction.canceled -= HandleMove;
            });
        }

        public void Update(float deltaTime)
        {
            _transform.Translate(_velocity * deltaTime);
        }

        private readonly PlayerStatus _status;
        private readonly SymphonyAnimeController _animeController;
        private readonly Rigidbody _rigidbody;
        private readonly NavMeshAgent _agent;
        private Transform _transform => _animeController.transform;

        private Vector3 _velocity;

        private void HandleMove(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            Vector3 dir = new(input.x, 0, input.y);
            float dirMag = dir.magnitude;

            if (0 < dirMag) // 移動方向がある時だけ向く。
            {
                _transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            }

            _velocity = dir * _status.MoveSpeed;
        }
    }
}
