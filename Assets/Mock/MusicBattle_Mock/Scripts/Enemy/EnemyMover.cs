using Mock.MusicBattle.Enemy;
using UnityEditorInternal;
using UnityEngine;

namespace Mock.MusicBattle.Enemy
{/// <summary>
 /// ロックオン判定と移動を行う。
 /// </summary>
    public class EnemyMover
    {
        
        public EnemyMover(Transform target,Transform enemy,EnemyStatus enemyStatus,Rigidbody rigidbody)
        {
            _targetposition = target;
            _enemyposition = enemy;
            _rigidbody = rigidbody;
            Init(enemyStatus);
        }

        public void Init(EnemyStatus enemyStatus)
        {
            _enemystatus = enemyStatus;
        }
        
        /// <summary>
        /// 射程内までプレイヤーに近づく。
        /// </summary>
        public void MoveTo()
        {
            if (_enemyposition == null ||_targetposition == null) return;

            float distance = Vector3.Distance(_targetposition.position, _enemyposition.position);
            //射程外　＝＞　近づく。
            if (distance > _enemystatus.AttackRange)
            {
                Vector3 direction = (_targetposition.position - _enemyposition.position).normalized;
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
        private Transform _targetposition;
        private Transform _enemyposition;
        
       
    }
}