using DevelopProducts.BehaviorGraph.Runtime.Adaptor;
using DevelopProducts.BehaviorGraph.Runtime.Adaptor.InGame.Battle;
using DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Enemy;
using DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Player;
using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Enemy;
using System;
using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     敵の動きを制御するコントローラークラス。
    /// </summary>
    public class EnemyAIControllerBG : IDisposable
    {
        public EnemyAIControllerBG(
            EnemyMoveUsecase enemyMoveUsecase,
            EnemyAttackReservationUsecase enemyAttackReservationUsecase,
            EnemyAttackUsecase enemyAttackUsecase,
            EnemyBattleStateBG enemyBattleState)
        {
            _enemyMoveUsecase = enemyMoveUsecase;
            _enemyAttackReservationUsecase = enemyAttackReservationUsecase;
            _enemyAttackUsecase = enemyAttackUsecase;
            _enemyBattleState = enemyBattleState;

            _enemyAttackReservationUsecase.OnReservedTimingReached += HandleReservedTimingReached;
        }

        // Debug用のイベント。
        public event Action OnAttackReserved;
        public event Action OnAttack;

        public EnemyMoveInstruction Tick(Vector3 enemyPosition, Vector3 targetPosition)
        {
            EnemyMoveDecision moveDecision = _enemyMoveUsecase.Evaluate(enemyPosition, targetPosition);
            Debug.Log($"[EnemyAIController] ShouldMove={moveDecision.ShouldMove}, IsInAttackRange={_enemyBattleState.IsInAttackRange}");

            if (moveDecision.ShouldMove)
            {
                if (_enemyBattleState.IsInAttackRange)
                {
                    Debug.Log("[EnemyAIController] 攻撃範囲から出たので予約キャンセル");
                    _enemyBattleState.ExitRange();
                    _enemyAttackReservationUsecase.Cancel();
                }

                return new EnemyMoveInstruction(
                    moveDecision.ShouldMove,
                    moveDecision.Destination,
                    moveDecision.Speed);
            }

            if (!_enemyBattleState.IsInAttackRange)
            {
                Debug.Log("[EnemyAIController] 攻撃範囲に入った");
                _enemyBattleState.EnterRange();
                ReserveAttack();
                //if (!_enemyAttackReservationUsecase.HasReservation)
                //{
                //    Debug.Log("[EnemyAIController] Encounter予約開始");
                //    _enemyAttackReservationUsecase.ReserveEncounter();
                //    OnAttackReserved?.Invoke();
                //}
            }

            return new EnemyMoveInstruction(
                    moveDecision.ShouldMove,
                    moveDecision.Destination,
                    moveDecision.Speed);
        }

        /// <summary>
        ///     攻撃を予約する。
        ///     Tickから抽出したもの。
        /// </summary>
        public void ReserveAttack()
        {
            if (!_enemyAttackReservationUsecase.HasReservation)
            {
                Debug.Log("[EnemyAIController] Encounter予約開始");
                _enemyAttackReservationUsecase.ReserveEncounter();
                OnAttackReserved?.Invoke();

                _enemyBattleState.AddEnemyStatus(EnumEnemyStatus.Aiming);
                _enemyBattleState.AddEnemyStatus(EnumEnemyStatus.Attacking);
            }
        }

        public void Dispose()
        {
            _enemyAttackReservationUsecase.OnReservedTimingReached -= HandleReservedTimingReached;
            _enemyAttackReservationUsecase.Dispose();
        }

        private void HandleReservedTimingReached()
        {
            Debug.Log("[EnemyAIController] HandleReservedTimingReached 呼ばれた");

            _enemyAttackUsecase.ExecuteAttack(
                _enemyBattleState.CurrentAttack,
                _enemyBattleState.Attacker,
                _enemyBattleState.Target);

            OnAttack?.Invoke();

            if (_enemyBattleState.IsInAttackRange)
            {
                _enemyAttackReservationUsecase.ReserveBattle();
                OnAttackReserved?.Invoke();
            }
            _enemyBattleState.RemoveEnemyStatus(EnumEnemyStatus.Aiming);
            _enemyBattleState.RemoveEnemyStatus(EnumEnemyStatus.Attacking);
        }

        private readonly EnemyMoveUsecase _enemyMoveUsecase;
        private readonly EnemyAttackReservationUsecase _enemyAttackReservationUsecase;
        private readonly EnemyAttackUsecase _enemyAttackUsecase;
        private readonly EnemyBattleStateBG _enemyBattleState;
    }
}
