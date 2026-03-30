using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;
using System;
using UnityEngine;
using static Codice.CM.Common.Purge.PurgeReport;

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

        public EnemyMoveInstruction Tick(Vector3 enemyPosition, Vector3 targetPosition)
        {
            EnemyMoveDecision moveDecision = _enemyMoveUsecase.Tick(enemyPosition, targetPosition);

            if (moveDecision.ShouldMove)
            {
                if (_enemyBattleState.IsInAttackRange)
                {
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
                _enemyBattleState.EnterRange();

                if (!_enemyAttackReservationUsecase.HasReservation)
                {
                    _enemyAttackReservationUsecase.ReserveEncounter();
                    OnAttackReserved?.Invoke();
                }
            }

            return new EnemyMoveInstruction(
                    moveDecision.ShouldMove,
                    moveDecision.Destination,
                    moveDecision.Speed);
        }

        public void Dispose()
        {
            _enemyAttackReservationUsecase.OnReservedTimingReached -= HandleReservedTimingReached;
            _enemyAttackReservationUsecase.Dispose();
        }

        private readonly EnemyMoveUsecase _enemyMoveUsecase;
        private readonly EnemyAttackReservationUsecase _enemyAttackReservationUsecase;
        private readonly EnemyAttackUsecase _enemyAttackUsecase;
        private readonly EnemyBattleState _enemyBattleState;

        private void HandleReservedTimingReached()
        {
            _enemyAttackUsecase.ExecuteAttack(
                _enemyBattleState.Attacker,
                _enemyBattleState.Target,
                _enemyBattleState.AttackId);

            OnAttack?.Invoke();

            if (_enemyBattleState.IsInAttackRange)
            {
                _enemyAttackReservationUsecase.ReserveBattle();
                OnAttackReserved?.Invoke();
            }
        }
    }
}
