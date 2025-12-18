using Mock.MusicBattle.Basis;
using System.Collections.Generic;
using Mock.MusicBattle.Character;
using Unity.Cinemachine;
using UnityEngine;
using Mock.MusicBattle.Battle;
using CriWare;
using System;
using Mock.MusicBattle.MusicSync;

namespace Mock.MusicBattle.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerManager : MonoBehaviour, ICharacter
    {
        public event Action<float> OnAttacked;

        public  Vector3 Pivot => _pivotTransform.position;
        public HealthEntity HealthEntity => _healthEntity;
        /// <summary>   inputBufferとCinemachineCameraの初期化。  </summary>
        public void Init(InputBuffer inputBuffer, CinemachineCamera CinemachineCamera,
            LockOnManager lockOnManager, MusicSyncManager musicSync)
        {
            _inputBuffer = inputBuffer;
            _lockOnManager = lockOnManager;
            Rigidbody rb = GetComponent<Rigidbody>();
            _animController = GetComponent<PlayerAnimationController>();
            _healthEntity = new HealthEntity(_playerStatus.MaxHealth);
            _playerAttacker = new PlayerAttacker(_playerStatus, _config, this);
            _playerMover = new PlayerMover(_playerStatus, rb, transform, CinemachineCamera.transform);
            _musicSyncManager = musicSync;
            InputEventRegister(_inputBuffer);
        }

        public void TakeDamage(float damage) => _healthEntity.TakeDamage(damage);

        [SerializeField, Tooltip("プレイヤーのステータス")]
        private PlayerStatus _playerStatus;
        [SerializeField, Tooltip("コンフィグ")]
        private PlayerConfig _config;
        [SerializeField, Tooltip("プレイヤーのピボット位置")]
        private Transform _pivotTransform;
        [SerializeField, Tooltip("銃声ソース")]
        private CriAtomSource _gunSoundSource;

        private HealthEntity _healthEntity;
        private InputBuffer _inputBuffer;
        private LockOnManager _lockOnManager;
        private PlayerMover _playerMover;
        private PlayerAttacker _playerAttacker;
        private PlayerAnimationController _animController;
        private MusicSyncManager _musicSyncManager;
        private Vector2 _input;
        private Vector3 _velocity;
        private HashSet<Collision> _hitGrounds = new();

        private void OnDisable()
        {
            if(_inputBuffer != null)
            InputEventUnregister(_inputBuffer);
        }

        private void Update()
        {
            if (_playerMover != null)
            {
                _velocity = _playerMover.CalcPlayerVelocityByInputDirection(_input);
                _animController?.MoveVelocity(_velocity.magnitude);
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
        private void OnDrawGizmos()
        {
            if (_playerAttacker != null)
                _playerAttacker.OnDrawGizmos();
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
            _healthEntity.OnDeath += OnDeathAction;
        }

        private void InputEventUnregister(InputBuffer inputBuffer)
        {
            inputBuffer.MoveAction.Performed -= OnInputMove;
            inputBuffer.MoveAction.Canceled -= OnInputMoveCancle;
            inputBuffer.AttackAction.Started -= OnInputAttack;
            _healthEntity.OnDeath -= OnDeathAction;
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
            if (_playerAttacker != null)
            {
                ICharacter target = _lockOnManager.LockOnTarget;
                float signature = _musicSyncManager.GetInputTimeSignature();
                _playerAttacker.Attack(target, signature);
                _gunSoundSource?.Play();
                OnAttacked?.Invoke(signature);
            }
        }

        private void OnDeathAction()
        {
            Debug.Log("Player Dead");
            gameObject.SetActive(false);
        }
    }
}
