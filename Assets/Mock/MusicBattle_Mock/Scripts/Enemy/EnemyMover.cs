using UnityEngine.AI;
using UnityEngine;
using System;

namespace Mock.MusicBattle.Enemy
{/// <summary>
 ///    ロックオン判定と移動を行う。
 /// </summary>
    public class EnemyMover
    {

        /// <summary> コンストラクタ。 </summary>
        public EnemyMover(Transform target, Transform enemy,
            EnemyStatus enemyStatus, NavMeshAgent agent, EnemyManager enemyManager)
        {
            _target = target;
            _enemy = enemy;
            _agent = agent;
            Init(enemyStatus);
            _enemyManager = enemyManager;
        }
        public event Action OnAttack;
        public event Action OnOutOfRange;

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
            if (!_agent.isOnNavMesh) return;
            if (_enemy == null || _target == null || _agent == null) return;

            float distance = DistanceToTarget();
            //射程外　＝＞　近づく。
            if (distance > _enemystatus.AttackRange)
            {
                if (_encount && !_inRange)
                {
                    OnOutOfRange?.Invoke();
                }
                _encount = false;
                _inRange = true;
                _agent.isStopped = false;
                _agent.SetDestination(_target.position);
            }
            //射程内＝＞止まって攻撃処理へ。
            else
            {
                if (_inRange && _enemyManager.IsLockOn)
                {
                    OnAttack?.Invoke();
                    _inRange = false;
                }
                _agent.isStopped = true;
                _encount = true;
            }
        }

        private EnemyManager _enemyManager;
        private EnemyStatus _enemystatus;
        private Transform _target;
        private Transform _enemy;
        private NavMeshAgent _agent;
        private bool _encount = false;
        private bool _inRange = false;

    }
}