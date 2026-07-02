using KillChord.Runtime.Adaptor.InGame.Enemy.EnemyAIFacadeInterface;
using KillChord.Runtime.Application.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.Utility.Persistent;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     ボスの行動を制御するコントローラー。
    /// </summary>
    public sealed class BossAIController : IDisposable
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        /// <param name="enemyMoveUsecase"> 移動意思決定のユースケース。 </param>
        /// <param name="reservationUsecase"> ボス専用の攻撃予約ユースケース。 </param>
        /// <param name="enemyBattleState"> AI判定用（移動・硬直・範囲）の戦闘状態。 </param>
        /// <param name="stateFacade"> 状態通知のファサード。 </param>
        /// <param name="patterns"> ボスが使用する攻撃パターン群（通常1/通常2/特殊1等）。 </param>
        public BossAIController(
            IDefender entity,
            EnemyMoveUsecase enemyMoveUsecase,
            BossAttackReservationUsecase reservationUsecase,
            EnemyBattleState enemyBattleState,
            IEnemyStateFacade stateFacade,
            Dictionary<Type, IRaycastDetectView> raycastViews,
            IReadOnlyList<BossAttackPattern> patterns)
        {
            _entity = entity;
            _enemyMoveUsecase = enemyMoveUsecase;
            _reservationUsecase = reservationUsecase;
            _enemyBattleState = enemyBattleState;
            _stateFacade = stateFacade;
            _patterns = patterns ?? throw new ArgumentNullException(nameof(patterns));
            _raycastViews = raycastViews ?? throw new ArgumentNullException(nameof(raycastViews));
            if (_patterns.Count == 0)
                throw new ArgumentException("攻撃パターンが1つもありません。", nameof(patterns));
            _lowHpEventTriggered = false;
            _isActive = false;

            _entity.OnHealthChanged += HandleHpChange;
        }

        // Debug／演出用イベント。
        /// <summary> 攻撃を予約時に発火するイベント </summary>
        public event Action OnAttackReserved;
        /// <summary> 攻撃を実行時に発火するイベント </summary>
        public event Action OnAttack;
        /// <summary> 攻撃の2拍前に発火するイベント </summary>
        public event Action On2BeatBefore;
        /// <summary> 攻撃の1拍前に発火するイベント </summary>
        public event Action On1BeatBefore;
        /// <summary> HPが一定量以下になった時のイベント </summary>
        public event Action OnLowHp;

        /// <summary> 攻撃中か。 </summary>
        public bool IsAttacking => _reservationUsecase.HasReservation;

        /// <summary>
        ///     有効化処理。
        /// </summary>
        public void Activate()
        {
            if (_isActive) return;
            _reservationUsecase.OnReservedTimingReached += HandleReservedTimingReached;
            _reservationUsecase.On2BeatBefore += Handle2BeatBefore;
            _reservationUsecase.On1BeatBefore += Handle1BeatBefore;
            EventBus<EOnTakeDamage>.Register(HandleOnDamageTaken);
            _isActive = true;
        }

        /// <summary>
        ///     無効化処理。
        /// </summary>
        public void Deactivate()
        {
            if (!_isActive) return;
            _reservationUsecase.OnReservedTimingReached -= HandleReservedTimingReached;
            _reservationUsecase.On2BeatBefore -= Handle2BeatBefore;
            _reservationUsecase.On1BeatBefore -= Handle1BeatBefore;
            _reservationUsecase.Deactivate();
            EventBus<EOnTakeDamage>.Unregister(HandleOnDamageTaken);
            _isActive = false;
        }

        /// <summary>
        ///     位置情報より行動意思を取得する。
        /// </summary>
        public EnemyMoveInstruction GetMoveInstruction(Vector3 enemyPosition, Vector3 targetPosition)
        {
            EnemyMoveDecision moveDecision = _enemyMoveUsecase.Evaluate(enemyPosition, targetPosition);
            if (moveDecision.ShouldMove)
            {
                if (_enemyBattleState.IsInAttackRange)
                {
                    _enemyBattleState.ExitRange();
                }
            }
            else
            {
                if (!_enemyBattleState.IsInAttackRange)
                {
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
        ///     予約前にパターンを選択し、そのタイミングで予約する。
        /// </summary>
        public void ReserveAttack()
        {
            if (_reservationUsecase.HasReservation) return;

            _current = SelectPattern();
            _reservationUsecase.Reserve(_current.Timing);
            OnAttackReserved?.Invoke();
        }

        /// <summary>
        ///     プレイヤーが敵の攻撃範囲内か取得する。
        /// </summary>
        public bool IsPlayerInAttackRange(Vector3 enemyPosition, Vector3 targetPosition)
        {
            return _enemyMoveUsecase.IsPlayerInAttackRange(enemyPosition, targetPosition);
        }

        /// <summary>
        ///     進行中の攻撃をキャンセルする。
        /// </summary>
        public void CancelAttack()
        {
            if (_reservationUsecase.HasReservation)
            {
                _reservationUsecase.Cancel();
            }
        }

        public void Dispose()
        {
            _entity.OnHealthChanged -= HandleHpChange;
            _reservationUsecase.OnReservedTimingReached -= HandleReservedTimingReached;
            _reservationUsecase.Dispose();
            EventBus<EOnTakeDamage>.Unregister(HandleOnDamageTaken);
            _reservationUsecase.On2BeatBefore -= Handle2BeatBefore;
            _reservationUsecase.On1BeatBefore -= Handle1BeatBefore;
        }

        /// <summary>
        ///     どの攻撃を撃つか選択する。
        ///     現状はランダム。順番制やHPフェーズ制にしたい場合はここを差し替える。
        /// </summary>
        private BossAttackPattern SelectPattern()
        {
            int index = UnityEngine.Random.Range(0, _patterns.Count);
            return _patterns[index];
        }

        /// <summary>
        ///     予約タイミング到達時の処理。選択中パターンのControllerで実行する。
        /// </summary>
        private void HandleReservedTimingReached()
        {
            _current?.Controller.ExecuteAttack();
            _enemyBattleState.AttackExcuted();
            if (_raycastViews.TryGetValue(_current.Controller.GetType(), out IRaycastDetectView view))
            {
                view.HideWarning();
            }
            OnAttack?.Invoke();
        }

        private void Handle2BeatBefore()
        {
            if (_raycastViews.TryGetValue(_current.Controller.GetType(), out IRaycastDetectView view))
            {
                view.StartTrackingWarning();
            }
            On2BeatBefore?.Invoke();
        }

        private void Handle1BeatBefore()
        {
            if (_raycastViews.TryGetValue(_current.Controller.GetType(), out IRaycastDetectView view))
            {
                view.LockWarningDirection();
            }
            On1BeatBefore?.Invoke();
        }

        /// <summary>
        ///     ダメージを受けた時の処理。クリティカル時は硬直する。
        /// </summary>
        private void HandleOnDamageTaken(EOnTakeDamage eventParam)
        {
            if (eventParam.DefenderId != _enemyBattleState.Attacker.Id) return;
            if (eventParam.Critical)
            {
                _enemyBattleState.Stunned();
                _stateFacade.Stunned();
            }
        }

        /// <summary>
        ///     ボスのHPが変わった時の処理。
        ///     半分以下になった時、イベントを発火する。
        ///     イベントの発火は1度のみとする。
        /// </summary>
        /// <param name="currentHp"></param>
        /// <param name="maxHp"></param>
        /// <param name="_"></param>
        private void HandleHpChange(float currentHp, float maxHp, float _)
        {
            if (_lowHpEventTriggered) return;
            if(currentHp < maxHp * LOW_HP_EVENT_TRIGGER_RATIO)
            {
                Debug.Log("<color=red>ボス体力が半分以下になった</color>");
                OnLowHp?.Invoke();
                _lowHpEventTriggered = true;
            }
        }

        private const float LOW_HP_EVENT_TRIGGER_RATIO = 0.5f;

        private readonly IDefender _entity;
        private readonly EnemyMoveUsecase _enemyMoveUsecase;
        private readonly BossAttackReservationUsecase _reservationUsecase;
        private readonly EnemyBattleState _enemyBattleState;
        private readonly IEnemyStateFacade _stateFacade;
        private readonly IReadOnlyList<BossAttackPattern> _patterns;
        private readonly Dictionary<Type, IRaycastDetectView> _raycastViews;
        private BossAttackPattern _current;
        private bool _lowHpEventTriggered;
        private bool _isActive;
    }
}
