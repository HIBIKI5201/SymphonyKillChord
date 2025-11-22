
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
        public event Action OnDeath
        {
            add => _healthEntity.OnDeath += value;
            remove => _healthEntity.OnDeath -= value;
        }
        public Transform LockTarget => _lockTarget;
        public void Awake()
        {
            _healthEntity = new HealthEntity(_enemyStatus.MaxHealth);
            Rigidbody rb = GetComponent<Rigidbody>();
            _enemyMover = new EnemyMover(_target, LockTarget, _enemyStatus, rb);
        }

        public void Init(EnemyStatus status)
        {
            _enemyStatus =  status;
        }
        public void TakeDamage(float damage) => _healthEntity.TakeDamage(damage);
        

        private void FixedUpdate()
        {
            if (_enemyMover == null) return;
            _enemyMover.MoveTo();
        }


        [SerializeField,Tooltip("エネミーのステータス")]
        private EnemyStatus _enemyStatus;
        [SerializeField,Tooltip("プレイヤーの位置")]
        private Transform _target;
        [SerializeField,Tooltip("敵の位置")]
        private Transform _lockTarget;
        
        private HealthEntity _healthEntity;
        private EnemyMover _enemyMover;
    }
}