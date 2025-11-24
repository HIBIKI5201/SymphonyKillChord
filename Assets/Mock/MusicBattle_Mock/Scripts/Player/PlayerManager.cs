using Mock.MusicBattle.Basis;
using Mock.MusicBattle.Camera;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Windows;

namespace Mock.MusicBattle
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerManager : MonoBehaviour
    {
        public void Init(InputBuffer inputBuffer)
        {
            _inputBuffer = inputBuffer;
            Rigidbody rb = GetComponent<Rigidbody>();
            CinemachineCamera CinemachineCamera = FindAnyObjectByType<CinemachineCamera>();
            _playerMover = new PlayerMover(_playerStatus, rb, transform, CinemachineCamera.transform);
            InputEventRegister(_inputBuffer);
        }

        public void TakeDamage(float damage)
        {
            // HelthEntityが未実装のため、仮実装
        }

        [SerializeField, Tooltip("プレイヤーのステータス")]
        private PlayerStatus _playerStatus;
        [SerializeField, Tooltip("コンフィグ")]
        private PlayerConfig _config;

        private InputBuffer _inputBuffer;
        private PlayerMover _playerMover;
        private PlayerAttacker _playerAttacker;
        private Vector2 _input;
        private Vector3 _velocity;
        private HashSet<Collision> _hitGrounds = new();

        private void OnDisable()
        {
            InputEventUnregister(_inputBuffer);
        }

        private void Update()
        {
            if (_playerMover != null)
            {
                _velocity = _playerMover.CalcPlayerVelocityByInputDirection(_input);
                _playerMover.SetPlayerVelocity(_velocity);
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
            if (_playerMover != null)
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

        private void InputEventRegister(InputBuffer inputBuffer)
        {
            if (inputBuffer == null)
            {
                Debug.LogError($"{nameof(InputBuffer)} is null");
                return;
            }
            inputBuffer.MoveAction.Performed += OnInputMove;
            inputBuffer.MoveAction.Canceled += OnInputMoveCancle;
            inputBuffer.AttackAction.Started += OnInputAttack;
        }

        private void InputEventUnregister(InputBuffer inputBuffer)
        {
            if (inputBuffer == null)
            {
                Debug.LogError($"{nameof(InputBuffer)} is null");
                return;
            }
            inputBuffer.MoveAction.Performed -= OnInputMove;
            inputBuffer.MoveAction.Canceled -= OnInputMoveCancle;
            inputBuffer.AttackAction.Started -= OnInputAttack;
        }

        private void OnInputMove(Vector2 input)
        {
            _input = input;
        }

        private void OnInputMoveCancle(Vector2 input)
        {
            _input = Vector2.zero;
        }

        private void OnInputAttack(float input)
        {

        }
    }
}
