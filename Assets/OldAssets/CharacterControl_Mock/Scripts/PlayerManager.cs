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

            // Animator取得
            if (animeController.TryGetComponent(out _animator))
            {
                _animator.applyRootMotion = false;
            }
            else
            {
                Debug.LogError($"{animeController.name} に {nameof(Animator)} がありません。");
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
            if (_velocity.sqrMagnitude > 0f)
                _transform.rotation = Quaternion.LookRotation(_velocity.normalized, Vector3.up);
        }


        // 攻撃アニメーションから呼び出される（Animation Event想定）
        public void OnAttackStart()
        {
            _isAttacking = true;
            _rigidbody.linearVelocity = Vector3.zero; // RootMotion中は入力移動を止める
        }

        public void OnAttackEnd()
        {
            _isAttacking = false;
        }

        // AnimatorのRootMotion反映
        public void OnAnimatorMove()
        {
            if (_isAttacking)
            {
                Vector3 delta = _animator.deltaPosition;
                Quaternion deltaRot = _animator.deltaRotation;

                _rigidbody.MovePosition(_rigidbody.position + delta);
                _rigidbody.MoveRotation(_rigidbody.rotation * deltaRot);
            }
        }

        private readonly PlayerStatus _status;
        private readonly SymphonyAnimeController _animeController;
        private readonly Transform _transform;
        private readonly Rigidbody _rigidbody;
        private readonly Animator _animator;

        private Vector3 _velocity;
        private bool _isAttacking = false;

        private void HandleMove(InputAction.CallbackContext context)
        {
            if (_isAttacking)
                return;

            Vector2 input = context.ReadValue<Vector2>();
            Vector3 dir = new(input.x, 0, input.y);
            float dirMag = dir.magnitude;

            if (dirMag > 0f)
            {
                Vector3 velocity = dir * _status.MoveSpeed;
                velocity.y = _rigidbody.linearVelocity.y;
                _velocity = velocity;
            }
            else
            {
                _velocity = Vector3.up * _rigidbody.linearVelocity.y;
            }
        }

        private void HandleAttack(InputAction.CallbackContext context)
        {
            if (_isAttacking)
                return;

            _animator.SetTrigger("Attack");
        }
    }
}
