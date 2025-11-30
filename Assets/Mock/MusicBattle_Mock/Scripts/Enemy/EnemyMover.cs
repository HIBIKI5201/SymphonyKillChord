using UnityEngine.AI;
using UnityEngine;

namespace Mock.MusicBattle.Enemy
{/// <summary>
 ///    ロックオン判定と移動を行う。
 /// </summary>
    public class EnemyMover
    {
        
        /// <summary> コンストラクタ。 </summary>
        public EnemyMover(Transform target,Transform enemy,
            EnemyStatus enemyStatus,NavMeshAgent agent)
        {
            _target = target;
            _enemy = enemy;
            _agent = agent;
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
        ///     射程内までプレイヤーに近づく。
        /// </summary>
        public void MoveTo()
        {
            if(!_agent.isOnNavMesh) return; 
            if (_enemy == null ||_target == null || _agent ==null) return;

            float distance =  DistanceToTarget();
            //射程外　＝＞　近づく。
            if (distance > _enemystatus.AttackRange)
            {
                _agent.isStopped = false;
                _agent.SetDestination(_target.position);
            }
            //射程内＝＞止まって攻撃処理へ。
            else
            {
                _agent.isStopped = true;
                
            }
        }
        
        private EnemyStatus  _enemystatus;
        private Transform _target;
        private Transform _enemy;
        private NavMeshAgent _agent;
        private bool _isDead = false;
      
    }
}