using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace Mock.CharacterControl
{
    /// <summary>
    ///     プレイヤーの管理クラス。
    ///     通常時はRigidbody制御、攻撃中はRootMotion制御を行う。
    /// </summary>
    public class PlayerManager
    {
        public PlayerManager(PlayerStatus status, SymphonyAnimeController animeController)
        {
            _status = status;

            if (animeController == null)
            {
                Debug.LogError("SymphonyAnimeController が指定されていません。");
                return;
            }

            _animeController = animeController;
            _transform = animeController.transform;
            animeController.OnAnimatorMoveAction += OnAnimatorMove;
            animeController.OnRootMotionChanged += isRootMotion =>
            {
                if (isRootMotion) { OnRollStart(); }
                else { OnRollEnd(); }
            };

            // Rigidbody取得・設定
            if (animeController.TryGetComponent(out _rigidbody))
            {
                _rigidbody.isKinematic = false;
            }
            else
            {
                Debug.LogError($"{animeController.name} に {nameof(Rigidbody)} がありません。");
            }
        }

        public void InputRegister(InputBuffer inputBuffer, CancellationToken token = default)
        {
            inputBuffer.MoveAction.performed += HandleMove;
            inputBuffer.MoveAction.canceled += HandleMove;

            inputBuffer.RollAction.performed += HandleRoll; // 攻撃入力

            token.Register(() =>
            {
                inputBuffer.MoveAction.performed -= HandleMove;
                inputBuffer.MoveAction.canceled -= HandleMove;
                inputBuffer.RollAction.performed -= HandleRoll;
            });
        }

        public void FixedUpdate(float deltaTime)
        {
            if (_isRolling) return;

            Vector3 velocity = GetVelocity();
            // 入力移動は Rigidbody.velocity で反映
            _rigidbody.linearVelocity = velocity;

            _animeController.MoveSpeed(_direction.magnitude);

            // 移動方向がほぼ水平の場合のみ回転
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
            if (horizontalVelocity.sqrMagnitude > 0.001f)
            {
                _transform.rotation = Quaternion.LookRotation(horizontalVelocity.normalized, Vector3.up);
            }
        }

        // AnimatorのRootMotion反映
        public void OnAnimatorMove()
        {
            if (_isRolling)
            {
                Vector3 delta = _animeController.DeltaPosition;
                Quaternion deltaRot = _animeController.DeltaRotation;

                _rigidbody.MovePosition(_rigidbody.position + delta);
                _rigidbody.MoveRotation(_rigidbody.rotation * deltaRot);
            }
        }

        private readonly PlayerStatus _status;
        private readonly SymphonyAnimeController _animeController;
        private readonly Transform _transform;
        private readonly Rigidbody _rigidbody;

        private Vector3 _direction;
        private bool _isRolling = false;

        private void HandleMove(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            _direction = new Vector3(input.x, 0f, input.y);
        }

        private void HandleRoll(InputAction.CallbackContext context)
        {
            if (_isRolling)
                return;

            _animeController.RollTrigger();
        }

        private void OnRollStart()
        {
            _isRolling = true;
        }

        private void OnRollEnd()
        {
            _isRolling = false;
        }

        private Vector3 GetVelocity()
        {
            Vector3 velocity = _direction * _status.MoveSpeed;
            velocity.y = _rigidbody.linearVelocity.y; // 落下速度を上書き。
            return velocity;
        }
    }
}
