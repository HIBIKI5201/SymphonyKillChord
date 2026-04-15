using DevelopProducts.BehaviorGraph.Runtime.Adaptor;
using DevelopProducts.BehaviorGraph.Runtime.Adaptor.InGame.Battle;
using DevelopProducts.BehaviorGraph.Runtime.Adaptor.InGame.Enemy;
using DevelopProducts.BehaviorGraph.Runtime.View.InGame.Enemy;
using Unity.Behavior;
using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.View.InGame
{
    public class EnemyAIFacade : MonoBehaviour
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        public void Initialize(EnemyMoveViewBG enemyMoveView, EnemyAIControllerBG enemyAIController, 
            EnemyBattleStateBG enemyBattleState, Transform targetTransform, Transform selfTransform)
        {
            _enemyMoveView = enemyMoveView;
            _enemyAIController = enemyAIController;
            _enemyBattleState = enemyBattleState;
            _targetTransform = targetTransform;
            _selfTransform = selfTransform;

            BehaviorGraphAgent behaviorAgent = GetComponent<BehaviorGraphAgent>();
            behaviorAgent.SetVariableValue("Enemy AI Facade", this);
            behaviorAgent.enabled = true;
        }

        private EnemyMoveViewBG _enemyMoveView;
        private EnemyAIControllerBG _enemyAIController;
        private EnemyBattleStateBG _enemyBattleState;
        private Transform _targetTransform;
        private Transform _selfTransform;

        public Vector3 TargetPosition => _targetTransform.position;

        public Vector3 SelfPosition => _selfTransform.position;

        public float DistanceToTarget => Vector3.Distance(_selfTransform.position, _targetTransform.position);

        public EnumEnemyStatus CurrentStatus => _enemyBattleState.EnemyStatus;

        /// <summary>
        ///     行動を停止し、待機状態になる。
        /// </summary>
        public void Hold()
        {
            // 実装予定あるか懸念。とりあえず枠を置いておく。
        }

        /// <summary>
        ///     行動指示を取得する。
        /// </summary>
        /// <returns></returns>
        public EnemyMoveInstruction GetMovementInstruction()
        {
            return _enemyAIController.Tick(_selfTransform.position, _targetTransform.position);
        }

        /// <summary>
        ///     敵の移動に関連する指示を出す。
        /// </summary>
        public void ApplyMovement(EnemyMoveInstruction intruction)
        {
            _enemyMoveView.ApplyMoveFromBG(intruction);
        }

        /// <summary>
        ///     攻撃行動に入る
        /// </summary>
        public void Attack()
        {
            Debug.Log("[EnemyAIFacade]敵攻撃開始");
            _enemyAIController.ReserveAttack();
        }

        /// <summary>
        ///     攻撃行動が中断された時の処理。
        ///     クリティカルヒット硬直などに使う。
        /// </summary>
        public void AttackInterrupted()
        {
            _enemyBattleState.RemoveEnemyStatus(EnumEnemyStatus.Aiming);
            _enemyBattleState.RemoveEnemyStatus(EnumEnemyStatus.Attacking);
        }

        /// <summary>
        ///     状態を最初に戻る。
        /// </summary>
        public void ResetStatus()
        {
            _enemyBattleState.SetEnemyStatus(EnumEnemyStatus.Idle);
        }

        /// <summary>
        ///     クリティカル攻撃を受けて硬直に入る
        /// </summary>
        public void CriticalStun()
        {
            // 実装待ち
            Debug.Log("[EnemyAIFacade]クリティカルヒット発生");
            AttackInterrupted();
        }
    }
}