using Mock.MusicBattle.Enemy;
using UnityEditorInternal;
using UnityEngine;

namespace Mock.MusicBattle.Enemy
{/// <summary>
 /// ロックオン判定と移動を行う。
 /// </summary>
    public class EnemyController
    {
        public bool IsLockedOn => _isLockedOn; 
        
        /// <summary>　ロックオン状態を設定する。</summary>
        public EnemyController(Transform target,Transform enemy,EnemyStatus enemyStatus,Rigidbody rigidbody)
        {
            _target = target;
            _enemy = enemy;
            _rigidbody = rigidbody;
            Init(enemyStatus);
        }
        public void SetLockOn(bool isLockedOn)
        {
            _isLockedOn = isLockedOn;
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        public void Init(EnemyStatus enemyStatus)
        {
            _enemystatus = enemyStatus;
        }

        public float DistanceToTarget()
        {
            return Vector3.Distance(_target.position, _enemy.position);
        }
        
        /// <summary>
        /// 射程内までプレイヤーに近づく。
        /// </summary>
        public void MoveTo()
        {
            if (_enemy == null ||_target == null) return;

            float distance =  DistanceToTarget();
            //射程外　＝＞　近づく。
            if (distance > _enemystatus.AttackRange)
            {
                Vector3 direction = (_target.position - _enemy.position).normalized;
                _rigidbody.linearVelocity = direction * _enemystatus.MoveSpeed;
            }
            //射程内＝＞止まって攻撃処理へ。
            else
            {
                _rigidbody.linearVelocity = Vector3.zero;
            }
        }
        private EnemyStatus  _enemystatus;
        private Rigidbody _rigidbody;
        private bool _isLockedOn = false;
        private Transform _target;
        private Transform _enemy;
        
       
    }
}