using UnityEngine.AI;
using UnityEngine;
using System;

namespace Mock.MusicBattle.Enemy
{
    /// <summary>
    ///     敵のロックオン判定と移動を行うクラス。
    /// </summary>
    public class EnemyMover
    {
        #region コンストラクタ
        /// <summary>
        ///     <see cref="EnemyMover"/>クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="target">追跡対象のTransform。</param>
        /// <param name="enemyTransform">敵自身のTransform。</param>
        /// <param name="enemyStatus">敵のステータス。</param>
        /// <param name="agent">敵のNavMeshAgent。</param>
        /// <param name="enemyManager">敵マネージャーの参照。</param>
        public EnemyMover(Transform target, Transform enemyTransform,
            EnemyStatus enemyStatus, NavMeshAgent agent, EnemyManager enemyManager)
        {
            _target = target;
            _enemyTransform = enemyTransform;
            _agent = agent;
            Init(enemyStatus);
            _enemyManager = enemyManager;
        }
        #endregion

        #region Publicイベント
        /// <summary> 敵が攻撃を行ったときに発火するイベント。 </summary>
        public event Action OnAttack;
        /// <summary> 敵が射程外に出たときに発火するイベント。 </summary>
        public event Action OnOutOfRange;
        #endregion

        #region Publicメソッド
        /// <summary>
        ///     初期化処理を行います。
        /// </summary>
        /// <param name="enemyStatus">敵のステータス。</param>
        public void Init(EnemyStatus enemyStatus)
        {
            _enemyStatus = enemyStatus;
        }

        /// <summary>
        ///     ターゲットとの距離を返します。
        /// </summary>
        /// <returns>ターゲットとの距離。</returns>
        public float DistanceToTarget()
        {
            return Vector3.Distance(_target.position, _enemyTransform.position);
        }

        /// <summary>
        ///     ターゲットへの移動を処理します。
        ///     射程外であればターゲットに近づき、射程内であれば停止して攻撃処理へ移行します。
        /// </summary>
        public void MoveTo()
        {
            if (!_agent.isOnNavMesh) return;
            if (_enemyTransform == null || _target == null || _agent == null) return;

            float distance = DistanceToTarget();
            // 射程外であればターゲットに近づく。
            if (distance > _enemyStatus.AttackRange)
            {
                if (_isEncountered && !_inRange)
                {
                    OnOutOfRange?.Invoke();
                }
                _isEncountered = false;
                _inRange = true;
                _agent.isStopped = false;
                _agent.SetDestination(_target.position);
            }
            // 射程内であれば停止して攻撃処理へ移行する。
            else
            {
                if (_inRange && _enemyManager.IsLockOn)
                {
                    OnAttack?.Invoke();
                    _inRange = false;
                }
                _agent.isStopped = true;
                _isEncountered = true;
            }
        }
        #endregion

        #region プライベートフィールド
        /// <summary> 敵マネージャーの参照。 </summary>
        private readonly EnemyManager _enemyManager;
        /// <summary> 敵のステータス。 </summary>
        private EnemyStatus _enemyStatus;
        /// <summary> 追跡対象のTransform。 </summary>
        private readonly Transform _target;
        /// <summary> 敵自身のTransform。 </summary>
        private readonly Transform _enemyTransform;
        /// <summary> 敵のNavMeshAgent。 </summary>
        private readonly NavMeshAgent _agent;
        /// <summary> 敵がターゲットと遭遇したかどうかを示すフラグ。 </summary>
        private bool _isEncountered = false;
        /// <summary> 敵が攻撃範囲内にいるかどうかを示すフラグ。 </summary>
        private bool _inRange = false;
        #endregion
    }
}
