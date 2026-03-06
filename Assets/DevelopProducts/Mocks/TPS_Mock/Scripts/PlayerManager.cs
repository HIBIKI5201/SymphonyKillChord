using System.Collections.Generic;
using UnityEngine;

namespace Mock.TPS
{
    /// <summary>
    ///     プレイヤーの管理クラス。
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerManager : MonoBehaviour, ICharacter
    {
        public void Init(InputBuffer inputBuffer, CameraManager cameraManager, HealthbarManager healthbarManager)
        {
            _inputBuffer = inputBuffer;

            Rigidbody rb = GetComponent<Rigidbody>();

            _playerMover = new PlayerMover(_playerStatus, rb,
                transform, Camera.main.transform);
            _playerAttacker = new PlayerAttacker(_playerStatus, _config, cameraManager.transform);
            _healthEntity = new HealthEntity(_playerStatus.MaxHealth);
            _healthEntity.OnHealthChanged += healthbarManager.SetHealthBar;

            InputEventRegister(inputBuffer);
        }

        public void TakeDamage(float damage) => _healthEntity.TakeDamage(damage);

        [SerializeField, Tooltip("プレイヤーのステータス")]
        private PlayerStatus _playerStatus;

        [SerializeField, Tooltip("コンフィグ")]
        private PlayerConfig _config;

        private InputBuffer _inputBuffer;
        private PlayerMover _playerMover;
        private PlayerAttacker _playerAttacker;
        private HealthEntity _healthEntity;

        private Vector3 _moveInput;
        private HashSet<Collision> _hitGrounds = new();

        private void OnDisable()
        {
            InputEventUnregister(_inputBuffer);
        }

        private void Update()
        {
            if (_playerMover != null)
            {
                Vector3 velocity = _playerMover.CalcPlayerVelocityByInputDirection(in _moveInput);
                _playerMover.SetPlayerVelocity(velocity);
                _playerMover.Update();
            }
        }

        private void FixedUpdate()
        {
            if (_playerMover != null)
            {
                _playerMover.FixedUpdate();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.contacts.Length == 0) { return; }

            // 衝突面の法線ベクトルを取得して、地面との接触かどうかを判定する。
            Vector3 contactNormal = collision.contacts[0].normal;
            if (Vector3.Dot(contactNormal, Vector3.up) > 0.5f)
            {
                _hitGrounds.Add(collision);
                _playerMover.SetIsGround(0 < _hitGrounds.Count);
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (_hitGrounds.Remove(collision))
            {
                // 地面との接触がなくなった場合、接地フラグを更新する。
                _playerMover.SetIsGround(0 < _hitGrounds.Count);
            }
        }

        private void OnDrawGizmos()
        {
            _playerAttacker?.OnDrawGizmos();
        }

        private void InputEventRegister(InputBuffer buffer)
        {
            if (buffer == null)
            {
                Debug.LogError($"{nameof(InputBuffer)} is null");
                return;
            }

            buffer.MoveAction.Performed += HandleInputMove;
            buffer.MoveAction.Canceled += HandleInputMove;
            buffer.JumpAction.Started += HandleJump;
            buffer.AttackAction.Started += HandleInputAttack;
        }

        private void InputEventUnregister(InputBuffer buffer)
        {
            if (buffer == null) { return; }

            buffer.MoveAction.Performed -= HandleInputMove;
            buffer.MoveAction.Canceled -= HandleInputMove;
            buffer.JumpAction.Started -= HandleJump;
            buffer.AttackAction.Started -= HandleInputAttack;
        }

        private void HandleInputMove(Vector2 input)
        {
            _moveInput = new Vector3(input.x, 0, input.y);
        }

        private void HandleJump(float input)
        {
            _playerMover.Jump();
        }

        private void HandleInputAttack(float input)
        {
            _playerAttacker.Attack();
        }
    }
}