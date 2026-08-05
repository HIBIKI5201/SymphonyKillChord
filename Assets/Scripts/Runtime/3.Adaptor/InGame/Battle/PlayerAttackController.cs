using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Utility.Persistent;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Battle
{
    /// <summary>
    ///     プレイヤーの攻撃アクションを制御するクラス。
    /// </summary>
    public class PlayerAttackController
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        /// <param name="attackIntervalEvaluator"></param>
        /// <param name="presenter"></param>
        /// <param name="battleState"></param>
        /// <param name="skillController"></param>
        /// <param name="targetingSystem"></param>
        /// <param name="musicSyncService"></param>
        /// <param name="targetAreaQuery"> 扇形範囲クエリです。 </param>
        /// <param name="playerTransform"> 判定の原点となるプレイヤーTransformです。 </param>
        public PlayerAttackController(
            AttackResultPresenter presenter,
            PlayerBattleState battleState,
            SkillController skillController,
            TargetSystemController targetingSystem,
            AttackIntervalEvaluator attackIntervalEvaluator,
            IMusicSyncService musicSyncService,
            MusicSyncState musicSyncState,
            TargetAreaQuery targetAreaQuery,
            Transform playerTransform,
            float attackRotationSpeed,
            float attackCooldown,
            int baseDamage
        )
        {
            _attackIntervalEvaluator = attackIntervalEvaluator;
            _presenter = presenter;
            _battleState = battleState;
            _skillController = skillController;
            _targetingSystem = targetingSystem;
            _musicSyncService = musicSyncService;
            _targetAreaQuery = targetAreaQuery;
            _playerTransform = playerTransform;
            AttackRotationSpeed = attackRotationSpeed;
            _baseDamage = baseDamage;

            _attackCooldown = attackCooldown * (60d / musicSyncState.Bpm);
            _attackCooldownRemainig = 0d;
        }

        /// <summary> プレイヤーが攻撃を実行したときに発火します。入力1回につき1回だけ発火します。 </summary>
        public event Action<string, bool> OnAttackExecuted;

        /// <summary> プレイヤーが指定拍子の攻撃を実行したときに発火します。 </summary>
        public event Action<BeatType> OnAttackBeatExecuted;

        /// <summary> 現在攻撃中かどうかを表すプロパティ。 </summary>
        public bool IsAttacking => _attackIntervalEvaluator.IsAttacking;

        /// <summary> 攻撃クールダウン中かどうかを表すプロパティ。 </summary>
        public bool IsAttackCooldown => _attackCooldownRemainig > 0f;

        /// <summary> 現在のロックオン対象。ロックオンしていない場合はnull。 </summary>
        public bool HasCurrentLockOnTarget { get; private set; }

        /// <summary> 現在のロックオン対象。ロックオンしていない場合はnull。 </summary>
        public Vector3 CurrentLockOnTargetPosition { get; private set; }

        /// <summary> 現在のロックオン対象。ロックオンしていない場合はnull。 </summary>
        public float AttackRotationSpeed { get; }

        /// <summary>
        ///     攻撃を実行する。
        /// </summary>
        /// <param name="resultBeatType"></param>
        /// <returns></returns>
        public bool ExecuteAttack(out int resultBeatType) //TODO : outでBeatTypeを返す構造を修正する
        {
            resultBeatType = 0;
            if (_targetingSystem == null)
            {
                Debug.LogError($"[{nameof(PlayerAttackController)}] TargetingSystemが設定されていません。");
                return false;
            }

            float now = Time.unscaledTime;
            BeatType beatType = _musicSyncService.GetCurrentBeatType();

            bool hasTarget = TryUpdateCurrentTarget();
            _skillController.TryExecuteSkill(BattleActionType.Attack, beatType, now);

            AttackDefinition attackDefinition = GetDifinitionByBeatType(beatType);   //攻撃定義未発見時にnullが返る

            if (attackDefinition == null) return false;

            StartAttackInterval();
            StartAttackCooldown();
            OnAttackBeatExecuted?.Invoke(beatType);
            resultBeatType = (int)beatType;

            // 前回の多段ヒットが残っている場合は破棄する。ヒット間隔が攻撃硬直より長い設定になっている。
            DiscardPendingHits(attackDefinition);

            // ロックオン対象がいない場合、仕様上は進行方向へ攻撃が発生するが敵にはHitしない。
            if (!hasTarget || !HasCurrentLockOnTarget)
            {
                OnAttackExecuted?.Invoke(attackDefinition.AttackName, false);
                return true;
            }

            if (!TryResolveHitTargets(attackDefinition))
            {
                OnAttackExecuted?.Invoke(attackDefinition.AttackName, false);
                return true;
            }

            bool hasHit = ApplyHit(attackDefinition);

            // 2発目以降は毎フレーム更新で消化する。
            int remainingHits = attackDefinition.HitCount - 1;
            if (remainingHits > 0)
            {
                _pendingAttackDefinition = attackDefinition;
                _pendingHitCount = remainingHits;
                _pendingHitTimer = attackDefinition.HitInterval;
            }

            OnAttackExecuted?.Invoke(attackDefinition.AttackName, hasHit);

            return true;
        }

        /// <summary>
        ///     攻撃インターバルを開始する。
        /// </summary>
        public void StartAttackInterval()
        {
            _attackIntervalEvaluator.EvaluateInterval();
        }

        /// <summary> 攻撃クールダウンを開始する。 </summary>
        public void StartAttackCooldown()
        {
            _attackCooldownRemainig = _attackCooldown;
        }

        /// <summary> 毎フレームクールダウンの減算と多段ヒットの消化を行う。 </summary>
        public void UpdateAttackCooldown(double deltaTime)
        {
            if (_attackCooldownRemainig > 0f)
            {
                _attackCooldownRemainig -= deltaTime;
                if (_attackCooldownRemainig < 0f)
                {
                    _attackCooldownRemainig = 0f;
                }
            }

            UpdatePendingHits(deltaTime);
        }

        /// <summary>
        ///     保留中の多段ヒットを消化する。
        /// </summary>
        /// <param name="deltaTime"> 前フレームからの経過秒数。 </param>
        private void UpdatePendingHits(double deltaTime)
        {
            if (_pendingHitCount <= 0)
            {
                return;
            }

            _pendingHitTimer -= deltaTime;
            if (_pendingHitTimer > 0d)
            {
                return;
            }

            AttackDefinition definition = _pendingAttackDefinition;
            _pendingHitCount--;
            _pendingHitTimer += definition.HitInterval;

            ApplyHit(definition);

            if (_pendingHitCount <= 0)
            {
                ClearPendingHits();
            }
        }

        /// <summary>
        ///     攻撃範囲内の対象を解決し、命中対象の一覧を作る。
        /// </summary>
        /// <param name="attackDefinition"> 攻撃定義。 </param>
        /// <returns> 命中対象が1体以上いる場合はtrue。 </returns>
        private bool TryResolveHitTargets(AttackDefinition attackDefinition)
        {
            _hitTargets.Clear();

            if (_targetAreaQuery == null || _playerTransform == null)
            {
                Debug.LogError($"[{nameof(PlayerAttackController)}] 範囲クエリまたはプレイヤーTransformが設定されていません。");
                return false;
            }

            // 判定の中心軸はプレイヤーからロックオン対象へ向かう方向とする。
            // 攻撃後に body が対象を向く仕様のため、transform.forward では判定時点で対象を向いていない。
            Vector3 origin = _playerTransform.position;
            Vector3 direction = CurrentLockOnTargetPosition - origin;

            _targetAreaQuery.QueryFanArea(
                origin,
                direction,
                attackDefinition.Range,
                attackDefinition.HalfAngleDegrees,
                _hitTargets);

            return _hitTargets.Count > 0;
        }

        /// <summary>
        ///     命中対象へ1ヒット分のダメージを適用する。
        ///     単体攻撃の場合は生存している最も近い1体のみを対象とする。
        /// </summary>
        /// <param name="attackDefinition"> 攻撃定義。 </param>
        /// <returns> 1体以上に命中した場合はtrue。 </returns>
        private bool ApplyHit(AttackDefinition attackDefinition)
        {
            _hitDefenders.Clear();

            // 一覧は水平距離の昇順。多段ヒットの途中で対象が倒れた場合は次に近い対象へ移る。
            for (int i = 0; i < _hitTargets.Count; i++)
            {
                TargetAreaHit hit = _hitTargets[i];
                if (!IsHitTargetAlive(hit))
                {
                    continue;
                }

                _hitDefenders.Add(hit.Entity);

                if (!attackDefinition.IsMultiTarget)
                {
                    break;
                }
            }

            if (_hitDefenders.Count == 0)
            {
                return false;
            }

            AttackExecutor.Execute(
                attackDefinition,
                _battleState.Attacker,
                _hitDefenders,
                false,
                _battleState.Attacker.BaseDamage,
                _hitResults);

            for (int i = 0; i < _hitResults.Count; i++)
            {
                AttackResult result = _hitResults[i];
                EventBus<EOnTakeDamage>.Raise(
                    new EOnTakeDamage(result.FinalDamage.Value, result.IsCritical, _hitDefenders[i].Id));

                _battleState.Attacker.SetDamage(result.FinalDamage);
                _presenter.Push(result);
            }

            return true;
        }

        /// <summary>
        ///     命中候補が生存しているかを判定する。
        /// </summary>
        /// <param name="hit"> 命中候補。 </param>
        /// <returns> 生存している場合はtrue。 </returns>
        private bool IsHitTargetAlive(in TargetAreaHit hit)
        {
            return hit.Entity != null
                && !hit.Entity.IsDead
                && hit.Target != null
                && hit.Target.IsAlive;
        }

        /// <summary>
        ///     保留中の多段ヒットを破棄する。
        /// </summary>
        /// <param name="nextAttackDefinition"> これから実行する攻撃定義。 </param>
        private void DiscardPendingHits(AttackDefinition nextAttackDefinition)
        {
            if (_pendingHitCount <= 0)
            {
                return;
            }

            Debug.LogWarning(
                $"[{nameof(PlayerAttackController)}] 前回の多段ヒットが完了する前に次の攻撃が実行されました。" +
                $"残り{_pendingHitCount}ヒットを破棄します。" +
                $"攻撃定義 '{nextAttackDefinition.AttackName}' のヒット間隔が攻撃硬直より長くないか確認してください。");

            ClearPendingHits();
        }

        /// <summary>
        ///     保留中の多段ヒットの状態を初期化する。
        /// </summary>
        private void ClearPendingHits()
        {
            _pendingHitCount = 0;
            _pendingHitTimer = 0d;
            _pendingAttackDefinition = null;
        }

        /// <summary>
        ///     現在のターゲット状態を更新します。
        /// </summary>
        /// <returns> 攻撃対象が存在する場合はtrueです。 </returns>
        private bool TryUpdateCurrentTarget()
        {
            if (!_targetingSystem.TryGetCurrentTargetEntity(out CharacterEntity targetEntity)
                && (!_targetingSystem.TryGetCurrentCandidateEntity(out targetEntity)
                    || !_targetingSystem.TrySetCurrentTarget(targetEntity.Id)))
            {
                _battleState.ClearTarget();
                HasCurrentLockOnTarget = false;
                CurrentLockOnTargetPosition = Vector3.zero;
                return false;
            }

            _battleState.ChangeTarget(targetEntity);

            if (_targetingSystem.TryGetCurrentTarget(out ITargetableViewModel lockOnTarget))
            {
                HasCurrentLockOnTarget = true;
                CurrentLockOnTargetPosition = lockOnTarget.Position;
            }
            else
            {
                HasCurrentLockOnTarget = false;
                CurrentLockOnTargetPosition = Vector3.zero;
            }

            return true;
        }

        private AttackDefinition GetDifinitionByBeatType(BeatType beatType)
        {
            try
            {
                return _battleState.Attacker.CombatSpec.GetAttackDefinitionByBeatType(beatType);
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogWarning(ex.Message);
                return null;
            }
        }

        private readonly AttackResultPresenter _presenter;
        private readonly PlayerBattleState _battleState;
        private readonly SkillController _skillController;
        private readonly TargetSystemController _targetingSystem;
        private readonly AttackIntervalEvaluator _attackIntervalEvaluator;
        private readonly IMusicSyncService _musicSyncService;
        private readonly TargetAreaQuery _targetAreaQuery;
        private readonly Transform _playerTransform;
        private readonly List<TargetAreaHit> _hitTargets = new List<TargetAreaHit>();
        private readonly List<CharacterEntity> _hitDefenders = new List<CharacterEntity>();
        private readonly List<AttackResult> _hitResults = new List<AttackResult>();
        private readonly int _baseDamage;
        private double _attackCooldownRemainig;
        private double _attackCooldown;
        private AttackDefinition _pendingAttackDefinition;
        private int _pendingHitCount;
        private double _pendingHitTimer;
    }
}
