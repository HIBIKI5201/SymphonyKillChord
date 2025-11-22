using Mock.MusicBattle.Enemy;
using UnityEditorInternal;
using UnityEngine;

namespace Mock.MusicBattle.Enemy
{/// <summary>
 ///    ロックオン判定と移動を行う。
 /// </summary>
    public class EnemyMover
    {
        
        /// <summary> ロックオン状態を設定する。 </summary>
        public EnemyMover(Transform target,Transform enemy,EnemyStatus enemyStatus,Rigidbody rigidbody)
        {
            _target = target;
            _enemy = enemy;
            _rigidbody = rigidbody;
            Init(enemyStatus);
        }
        
        /// <summary> 初期化処理をする。 </summary>
        public void Init(EnemyStatus enemyStatus)
        {
            _enemystatus = enemyStatus;
        }
        
        /// <summary> ターゲットとの距離を返す。 </summary>
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
        private Transform _target;
        private Transform _enemy;
        private bool _isLockedOn = false;
        
       
    }
}