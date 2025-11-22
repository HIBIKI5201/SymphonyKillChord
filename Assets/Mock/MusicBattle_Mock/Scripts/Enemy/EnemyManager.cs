using System;
using UnityEngine;
using Mock.MusicBattle.Character;


namespace Mock.MusicBattle.Enemy
{
    /// <summary>
    /// 敵の主要な処理を一括して管理するクラス。
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyManager : MonoBehaviour
    {
        /// <summary>ヘルスが0になったときに通知。</summary>
        public event Action OnDeath;
        public Transform LockTarget => _lockTarget;

        public void Awake()
        {
            _lockTarget = transform;
            _rb = GetComponent<Rigidbody>();
        }

        public void SetStatus(EnemyStatus status)
        {
            _enemyStatus = status;
            _healthEntity = new HealthEntity(_enemyStatus.MaxHealth);
            _healthEntity.OnDeath += () => OnDeath?.Invoke();
        }

        public void SetTarget(Transform target)
        {
            _target = target;
            InitializeMover();
        }

        private void InitializeMover()
        {
            if (_enemyStatus == null || _target == null)
            {
                Debug.Log("EnemyStatus/Target is null");
                return;
            }

            _enemyMover = new EnemyMover(_target, _lockTarget, _enemyStatus, _rb);
        }

        public void TakeDamage(float damage) => _healthEntity.TakeDamage(damage);


        private void FixedUpdate()
        {
            if (_enemyMover == null)
            {
                Debug.Log("EnemyMove is null");
                return;
            }
            _enemyMover.MoveTo();
        }


        [SerializeField, Tooltip("エネミーのステータス")]
        private EnemyStatus _enemyStatus;
        private Transform _target;
        private Transform _lockTarget;
        private Rigidbody _rb;
        private HealthEntity _healthEntity;
        private EnemyMover _enemyMover;
    }
}