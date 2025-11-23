using Mock.MusicBattle.Basis;
using Mock.MusicBattle.Camera;
using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace Mock.MusicBattle
{
    public class PlayerManager : MonoBehaviour
    {
        public void Init(InputBuffer inputBuffer, CameraManager cameraManager)
        {
            _inputBuffer = inputBuffer;
            Rigidbody rb = GetComponent<Rigidbody>();
            CinemachineCamera CinemachineCamera = GetComponent<CinemachineCamera>();
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
        private HashSet<Collision> _hitGrounds = new();

        private void OnDisable()
        {
            InputEventUnregister(_inputBuffer);
        }

        private void InputEventRegister(InputBuffer inputBuffer)
        {
            if (inputBuffer == null)
            {
                Debug.LogError($"{nameof(InputBuffer)} is null");
                return;
            }
            inputBuffer.MoveAction.Performed += OnInputMove;
            inputBuffer.MoveAction.Canceled += OnInputMove;
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
            inputBuffer.MoveAction.Canceled -= OnInputMove;
            inputBuffer.AttackAction.Started -= OnInputAttack;
        }

        private void OnInputMove(Vector2 input)
        {

        }

        private void OnInputAttack(float input)
        {

        }
    }
}
