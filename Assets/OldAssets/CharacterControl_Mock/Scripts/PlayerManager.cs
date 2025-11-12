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
                if (isRootMotion) { OnAttackStart(); }
                else { OnAttackEnd(); }
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

            inputBuffer.AttackAction.performed += HandleAttack; // 攻撃入力

            token.Register(() =>
            {
                inputBuffer.MoveAction.performed -= HandleMove;
                inputBuffer.MoveAction.canceled -= HandleMove;
                inputBuffer.AttackAction.performed -= HandleAttack;
            });
        }

        public void Update(float deltaTime)
        {
            if (_isAttacking) return;

            // 入力移動は Rigidbody.velocity で反映
            _rigidbody.linearVelocity = _velocity;

            // 移動方向がほぼ水平の場合のみ回転
            Vector3 horizontalVelocity = new Vector3(_velocity.x, 0f, _velocity.z);
            if (horizontalVelocity.sqrMagnitude > 0.001f)
            {
                _transform.rotation = Quaternion.LookRotation(horizontalVelocity.normalized, Vector3.up);
            }
        }

        // AnimatorのRootMotion反映
        public void OnAnimatorMove()
        {
            if (_isAttacking)
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

        private Vector3 _velocity;
        private bool _isAttacking = false;

        private void HandleMove(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            Vector3 dir = new Vector3(input.x, 0f, input.y);
            float dirMag = dir.magnitude;

            Vector3 horizontalVelocity = Vector3.zero;

            if (dirMag > 0f)
            {
                horizontalVelocity = dir.normalized * _status.MoveSpeed;
            }

            // Y方向はRigidbodyの現在の速度を保持（重力反映）
            _velocity = new Vector3(horizontalVelocity.x, _rigidbody.linearVelocity.y, horizontalVelocity.z);
        }

        private void HandleAttack(InputAction.CallbackContext context)
        {
            if (_isAttacking)
                return;

            _animeController.AttackTrigger();
        }

        private void OnAttackStart()
        {
            _isAttacking = true;
        }

        private void OnAttackEnd()
        {
            _isAttacking = false;
        }
    }
}
