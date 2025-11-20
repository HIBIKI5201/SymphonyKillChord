using Mock.MusicBattle.Enemy;
using UnityEditorInternal;
using UnityEngine;

namespace Mock.MusicBattle
{/// <summary>
 /// ロックオン判定と移動を行う。
 /// </summary>
    public class EnemyLockOnController
    {
        public EnemyLockOnController(Transform target,Transform enemy,EnemyStatus enemyStatus,Rigidbody rigidbody)
        {
            _targetposition = target;
            _enemyposition = enemy;
            _rigidbody = rigidbody;
            Init(enemyStatus);
        }

        private void Init(EnemyStatus enemyStatus)
        {
            _moveSpeed = enemyStatus.MoveSpeed;
            _attackRange = enemyStatus.AttackRange;
        }

        /// <summary> プレイヤーを設定する。</summary>
        public void SetTarget(Transform target)
        {
            _targetposition = target;
        }

        /// <summary>　ロックオン対象にする。</summary>
        public void SetLockOn()
        {
            _isLockedOn = true;
        }

        /// <summary>
        /// 射程内までプレイヤーに近づく。
        /// </summary>
        private void MoveTo()
        {
            if (_enemyposition == null ||_targetposition == null) return;

            float distance = Vector3.Distance(_targetposition.position, _enemyposition.position);
            //射程外　＝＞　近づく。
            if (distance > _attackRange)
            {
                Vector3 direction = (_targetposition.position - _enemyposition.position).normalized;
                _rigidbody.linearVelocity = direction * _moveSpeed;
            }
            //射程内＝＞止まって攻撃処理へ。
            else
            {
                _rigidbody.linearVelocity = Vector3.zero;
            }
        }
        
        private float _moveSpeed;
        private float _attackRange;
        private Rigidbody _rigidbody;
        private bool _isLockedOn = false;
        private Transform _targetposition;
        private Transform _enemyposition;
    }
}