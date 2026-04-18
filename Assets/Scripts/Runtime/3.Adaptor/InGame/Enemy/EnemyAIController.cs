using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Application.InGame.Enemy;
using KillChord.Runtime.Application.InGame.Player;
using KillChord.Runtime.Domain.InGame.Enemy;
using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     敵の動きを制御するコントローラークラス。
    /// </summary>
    public class EnemyAIController : IDisposable
    {
        public EnemyAIController(
            EnemyMoveUsecase enemyMoveUsecase,
            EnemyAttackReservationUsecase enemyAttackReservationUsecase,
            EnemyAttackUsecase enemyAttackUsecase,
            EnemyBattleState enemyBattleState)
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
            _enemyBattleState.AttackExcuted();

            OnAttack?.Invoke();
        }

        private readonly EnemyMoveUsecase _enemyMoveUsecase;
        private readonly EnemyAttackReservationUsecase _enemyAttackReservationUsecase;
        private readonly EnemyAttackUsecase _enemyAttackUsecase;
        private readonly EnemyBattleState _enemyBattleState;
    }
}
