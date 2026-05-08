using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Application.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.Utility;
using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     敵の動きを制御するコントローラークラス。
    /// </summary>
    public class EnemyAIController : IDisposable
    {
        public EnemyAIController(
            EnemyMoveUsecase enemyMoveUsecase,
            EnemyAttackReservationUsecase enemyAttackReservationUsecase,
            EnemyBattleState enemyBattleState,
            IEnemyStateFacade stateFacade,
            IEnemyAttackController attackController
            )
        {
            _enemyMoveUsecase = enemyMoveUsecase;
            _enemyAttackReservationUsecase = enemyAttackReservationUsecase;
            _enemyBattleState = enemyBattleState;
            _stateFacade = stateFacade;
            _attackController = attackController;

            _enemyAttackReservationUsecase.OnReservedTimingReached += HandleReservedTimingReached;
            EventBus<EOnTakeDamage>.Register(HandleOnDamageTaken);
        }

        // Debug用のイベント。
        /// <summary> 攻撃を予約時に発火するイベント </summary>
        public event Action OnAttackReserved;
        /// <summary> 攻撃を実行時に発火するイベント </summary>
        public event Action OnAttack;

        /// <summary> 敵が攻撃中か。 </summary>
        public bool IsAttacking => _enemyAttackReservationUsecase.HasReservation;

        /// <summary>
        ///     位置情報より行動意思を取得する。
        /// </summary>
        /// <param name="enemyPosition"></param>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        public EnemyMoveInstruction GetMoveInstruction(Vector3 enemyPosition, Vector3 targetPosition)
        {
            EnemyMoveDecision moveDecision = _enemyMoveUsecase.Evaluate(enemyPosition, targetPosition);
            if (moveDecision.ShouldMove)
            {
                if (_enemyBattleState.IsInAttackRange)
                {
                    Debug.Log("[EnemyAIController] 攻撃範囲を出た");
                    _enemyBattleState.ExitRange();
                }
            }
            else
            {
                if (!_enemyBattleState.IsInAttackRange)
                {
                    Debug.Log("[EnemyAIController] 攻撃範囲に入った");
                    _enemyBattleState.EnterRange();
                }
            }
            return new EnemyMoveInstruction(
            moveDecision.ShouldMove,
            moveDecision.Destination,
            moveDecision.Speed);
        }

        /// <summary>
        ///     攻撃を予約する。
        /// </summary>
        public void ReserveAttack()
        {
            if (!_enemyAttackReservationUsecase.HasReservation)
            {
                Debug.Log("[EnemyAIController] Encounter予約開始");
                if (_enemyBattleState.FirstAttack)
                {
                    // 初回攻撃
                    _enemyAttackReservationUsecase.ReserveEncounter();
                }
                else
                {
                    // 2回目以降の攻撃
                    _enemyAttackReservationUsecase.ReserveBattle();
                }
                OnAttackReserved?.Invoke();
            }
        }

        /// <summary>
        ///     プレイヤーが敵の攻撃範囲内か取得する。
        /// </summary>
        /// <param name="enemyPosition"></param>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        public bool IsPlayerInAttackRange(Vector3 enemyPosition, Vector3 targetPosition)
        {
            return _enemyMoveUsecase.IsPlayerInAttackRange(enemyPosition, targetPosition);
        }

        /// <summary>
        ///     進行中の攻撃をキャンセルする。
        /// </summary>
        public void CancelAttack()
        {
            if (_enemyAttackReservationUsecase.HasReservation)
            {
                _enemyAttackReservationUsecase.Cancel();
            }
        }

        public void Dispose()
        {
            _enemyAttackReservationUsecase.OnReservedTimingReached -= HandleReservedTimingReached;
            _enemyAttackReservationUsecase.Dispose();

            EventBus<EOnTakeDamage>.Unregister(HandleOnDamageTaken);
        }

        /// <summary>
        ///     予約タイミングが到達した時に実行される処理。
        /// </summary>
        private void HandleReservedTimingReached()
        {
            _attackController.ExecuteAttack();
            _enemyBattleState.AttackExcuted();
            OnAttack?.Invoke();
        }

        /// <summary>
        ///     ダメージを受ける時の処理。
        /// </summary>
        /// <param name="eventParam"></param>
        private void HandleOnDamageTaken(EOnTakeDamage eventParam)
        {
            if (eventParam.DefenderId != _enemyBattleState.Attacker.Id) return;
            // クリティカル発生時、硬直行動をする
            if (eventParam.Critical)
            {
                _enemyBattleState.Stunned();
                _stateFacade.Stunned();
            }
        }

        private readonly EnemyMoveUsecase _enemyMoveUsecase;
        private readonly EnemyAttackReservationUsecase _enemyAttackReservationUsecase;
        private readonly EnemyBattleState _enemyBattleState;
        private readonly IEnemyStateFacade _stateFacade;
        private readonly IEnemyAttackController _attackController;
    }
}
