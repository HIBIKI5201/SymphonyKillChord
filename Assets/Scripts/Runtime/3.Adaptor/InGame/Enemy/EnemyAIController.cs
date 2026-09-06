using KillChord.Runtime.Adaptor.InGame.Enemy.EnemyAIFacadeInterface;
using KillChord.Runtime.Application.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.Utility.Persistent;
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
            IEnemyAttackController attackController,
            EnemyPostAttackBehaviorUsecase postAttackBehaviorUsecase,
            ObstacleSearchService obstacleSearchService,
            EnemyAIControllerRegistry registry
            )
        {
            _enemyMoveUsecase = enemyMoveUsecase;
            _enemyAttackReservationUsecase = enemyAttackReservationUsecase;
            _enemyBattleState = enemyBattleState;
            _stateFacade = stateFacade;
            _attackController = attackController;
            _postAttackBehaviorUsecase = postAttackBehaviorUsecase;
            _obstacleSearchService = obstacleSearchService;
            _registry = registry;
            _isActive = false;
        }

        /// <summary>
        ///     有効化処理。
        /// </summary>
        public void Activate()
        {
            if (_isActive) return;
            _enemyAttackReservationUsecase.OnReservedTimingReached += HandleReservedTimingReached;
            _enemyAttackReservationUsecase.On2BeatBefore += Handle2BeatBefore;
            _enemyAttackReservationUsecase.On1BeatBefore += Handle1BeatBefore;
            EventBus<EOnTakeDamage>.Register(HandleOnDamageTaken);
            _isActive = true;
        }

        /// <summary>
        ///     無効化処理。
        /// </summary>
        public void Deactivate()
        {
            if (!_isActive) return;
            _enemyAttackReservationUsecase.OnReservedTimingReached -= HandleReservedTimingReached;
            _enemyAttackReservationUsecase.On2BeatBefore -= Handle2BeatBefore;
            _enemyAttackReservationUsecase.On1BeatBefore -= Handle1BeatBefore;
            _enemyAttackReservationUsecase.Deactivate();
            EventBus<EOnTakeDamage>.Unregister(HandleOnDamageTaken);
            _isActive = false;
        }
        /// <summary>
        ///     戦闘系行動に関するAIを有効にする。
        /// </summary>
        public void StartBattleAI()
        {
            _enemyBattleState.StartBattleAI();
        }

        /// <summary>
        ///     戦闘系行動に関するAIを無効にする。
        /// </summary>
        public void StopBattleAI()
        {
            CancelAttack();
            _enemyBattleState.StopBattleAI();
        }

        // Debug用のイベント。
        /// <summary> 攻撃を予約時に発火するイベント </summary>
        public event Action OnAttackReserved;
        /// <summary> 攻撃を実行時に発火するイベント </summary>
        public event Action OnAttack;
        /// <summary>   攻撃の2拍前に発火するイベント   </summary>
        public event Action On2BeatBefore;
        /// <summary>   攻撃の1拍前に発火するイベント </summary>
        public event Action On1BeatBefore;

        /// <summary> 敵が攻撃中か。 </summary>
        public bool IsAttacking => _enemyAttackReservationUsecase.HasReservation;
        /// <summary> 直近に取得した自身の位置。 </summary>
        public Vector3 CurrentPosition => _lastKnownPosition;

        /// <summary>
        ///     位置情報より行動意思を取得する。
        /// </summary>
        /// <param name="enemyPosition"></param>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        public EnemyMoveInstruction GetMoveInstruction(Vector3 enemyPosition, Vector3 targetPosition)
        {
            _lastKnownPosition = enemyPosition;

            // 攻撃後の行動選択で移動先が上書きされている場合、そちらを優先する
            if (_enemyBattleState.OverrideDestination.HasValue)
            {
                Vector3 overrideDestination = _enemyBattleState.OverrideDestination.Value;
                if (Vector3.Distance(enemyPosition, overrideDestination) > _postAttackBehaviorUsecase.ArrivalThreshold)
                {
                    return new EnemyMoveInstruction(true, overrideDestination, _enemyMoveUsecase.MoveSpeed);
                }
                _enemyBattleState.ClearOverrideDestination();
            }

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
            ChoosePostAttackBehavior();
            OnAttack?.Invoke();
        }

        /// <summary>
        ///     攻撃の2拍前に到達した時に実行される処理。
        /// </summary>

        private void Handle2BeatBefore()
        {
            Debug.Log("[EnemyAIController] 攻撃の2拍前");
            On2BeatBefore?.Invoke();
        }

        /// <summary>
        ///     攻撃の1拍前に到達した時に実行される処理。
        /// </summary>
        private void Handle1BeatBefore()
        {
            Debug.Log("[EnemyAIController] 攻撃の1拍前");
            On1BeatBefore?.Invoke();
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

        /// <summary>
        ///     攻撃後の行動(再攻撃/合流/障害物接近)を選択し、必要であれば移動先を上書きする。
        /// </summary>
        private void ChoosePostAttackBehavior()
        {
            Vector3? allyPosition = _registry != null && _registry.TryFindNearestOtherActive(this, _lastKnownPosition, out EnemyAIController nearestAlly)
                ? nearestAlly.CurrentPosition
                : (Vector3?)null;

            Vector3? obstaclePosition = _obstacleSearchService.TryFindNearestObstaclePosition(_lastKnownPosition, out Vector3 nearestObstacle)
                ? nearestObstacle
                : (Vector3?)null;

            if (_postAttackBehaviorUsecase.TryDecideOverrideDestination(_lastKnownPosition, allyPosition, obstaclePosition, out Vector3 overrideDestination))
            {
                _enemyBattleState.SetOverrideDestination(overrideDestination);
            }
        }

        private readonly EnemyMoveUsecase _enemyMoveUsecase;
        private readonly EnemyAttackReservationUsecase _enemyAttackReservationUsecase;
        private readonly EnemyBattleState _enemyBattleState;
        private readonly IEnemyStateFacade _stateFacade;
        private readonly EnemyPostAttackBehaviorUsecase _postAttackBehaviorUsecase;
        private readonly ObstacleSearchService _obstacleSearchService;
        private readonly EnemyAIControllerRegistry _registry;
        private IEnemyAttackController _attackController;
        private bool _isActive;
        private Vector3 _lastKnownPosition;
    }
}
