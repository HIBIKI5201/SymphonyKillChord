using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Utility.Diagnostics;
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
        /// <param name="attackPresenter"> 攻撃成立の判定結果を表示側へ渡すPresenter。 </param>
        /// <param name="battleState"></param>
        /// <param name="skillController"></param>
        /// <param name="targetingSystem"></param>
        /// <param name="musicSyncService"></param>
        /// <param name="targetAreaQuery"> 扇形範囲クエリです。 </param>
        /// <param name="playerTransform"> 判定の原点となるプレイヤーTransformです。 </param>
        /// <param name="pendingAttackEffectService"> 攻撃の多段ヒットを管理するサービスです。 </param>
        /// <param name="attackOcclusion"> 攻撃が障害物に遮られるかを判定するViewModelです。 </param>
        /// <param name="attackCooldown"> 回避の後に攻撃できない時間（拍数）です。 </param>
        /// <param name="attackRecoveryBeats"> 通常攻撃の攻撃硬直（拍数）です。スキルが発動した攻撃はスキルの値を使います。 </param>
        public PlayerAttackController(
            AttackResultPresenter presenter,
            PlayerAttackPresenter attackPresenter,
            PlayerBattleState battleState,
            PlayerActionRestrictionState actionRestrictionState,
            SkillController skillController,
            TargetSystemController targetingSystem,
            AttackIntervalEvaluator attackIntervalEvaluator,
            IMusicSyncService musicSyncService,
            MusicSyncState musicSyncState,
            TargetAreaQuery targetAreaQuery,
            Transform playerTransform,
            PendingAttackEffectService pendingAttackEffectService,
            IAttackOcclusionViewModel attackOcclusion,
            float attackRotationSpeed,
            float attackCooldown,
            AttackRecoveryBeats attackRecoveryBeats
        )
        {
            _attackIntervalEvaluator = attackIntervalEvaluator;
            _presenter = presenter;
            _attackPresenter = attackPresenter ?? throw new ArgumentNullException(nameof(attackPresenter));
            _battleState = battleState;
            _actionRestrictionState = actionRestrictionState;
            _skillController = skillController;
            _targetingSystem = targetingSystem;
            _musicSyncService = musicSyncService;
            _musicSyncState = musicSyncState;
            _targetAreaQuery = targetAreaQuery;
            _playerTransform = playerTransform;
            _pendingAttackEffectService = pendingAttackEffectService ?? throw new ArgumentNullException(nameof(pendingAttackEffectService));
            _attackOcclusion = attackOcclusion ?? throw new ArgumentNullException(nameof(attackOcclusion));
            AttackRotationSpeed = attackRotationSpeed;

            _attackCooldown = attackCooldown * (60d / musicSyncState.Bpm);
            _attackCooldownRemainig = 0d;
            _attackRecoveryBeats = attackRecoveryBeats;
            CurrentRecoveryBeats = attackRecoveryBeats.Value;
        }

        /// <summary> プレイヤーが攻撃を実行したときに発火します。入力1回につき1回だけ発火します。 </summary>
        public event Action<string, bool> OnAttackExecuted;

        /// <summary> ミッション記録向けに成立した攻撃の拍種を通知します。 </summary>
        public event Action<BeatType> OnAttackBeatExecuted;

        /// <summary>
        ///     多段攻撃の2発目以降を撃ったときに発火します。
        ///     命中の有無やロックオン対象の有無は問わず、ヒット間隔ごとに発火します。
        /// </summary>
        public event Action OnFollowUpShotFired;

        /// <summary> 現在攻撃中かどうかを表すプロパティ。 </summary>
        public bool IsAttacking => _attackIntervalEvaluator.IsAttacking;

        /// <summary> 回避後の攻撃クールダウン中かどうかを表すプロパティ。 </summary>
        public bool IsAttackCooldown => _attackCooldownRemainig > 0f;

        /// <summary> ロックオン対象が存在する場合はtrue。 </summary>
        public bool HasCurrentLockOnTarget { get; private set; }

        /// <summary> ロックオン対象の座標。対象がない場合は <see cref="Vector3.zero"/>。 </summary>
        public Vector3 CurrentLockOnTargetPosition { get; private set; }

        /// <summary> 攻撃時に対象へ向き直る回転速度。 </summary>
        public float AttackRotationSpeed { get; }

        /// <summary> 直近の攻撃で使った攻撃硬直の拍数。攻撃前は通常攻撃の値。 </summary>
        public float CurrentRecoveryBeats { get; private set; }

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
            float musicTime = (float)_musicSyncState.PlayTime;
            BeatType beatType = _musicSyncService.GetCurrentBeatType(out bool isJustHit);

            bool hasTarget = TryUpdateCurrentTarget();

            var normalAttackDamagePolicy = _skillController.TryExecuteSkill(BattleActionType.Attack, beatType, now, musicTime, isJustHit, _actionRestrictionState.CanUseSkill, out AttackRecoveryBeats? skillRecoveryBeats);

            IAttackHitEffect[] pendingHitEffects = _pendingAttackEffectService.Consume();

            AttackDefinition attackDefinition = GetDifinitionByBeatType(beatType);   //攻撃定義未発見時にnullが返る

            if (attackDefinition == null)
            {
                return false;
            }

            // スキルが発動した攻撃はスキルの硬直、それ以外は通常攻撃の硬直を拍数で使う。
            StartAttackInterval(skillRecoveryBeats ?? _attackRecoveryBeats);
            OnAttackBeatExecuted?.Invoke(beatType);
            _attackPresenter.Push(beatType, isJustHit);
            resultBeatType = (int)beatType;

            // 攻撃演出用に、攻撃が成立したことを通知する。命中の有無は問わない。
            EventBus<EOnPlayerAttackExecuted>.Raise(new EOnPlayerAttackExecuted());

            // 前回の多段ヒットが残っている場合は破棄する。ヒット間隔が攻撃硬直より長い設定になっている。
            DiscardPendingHits(attackDefinition);

            bool hasHit = false;
            bool shouldApplyPendingHits = false;

            // ロックオン対象がいない場合は正面へ射程まで撃ち、途中の敵に当てる。
            if (TryResolveHitTargets(attackDefinition, hasTarget && HasCurrentLockOnTarget))
            {
                if (normalAttackDamagePolicy == SkillNormalAttackDamagePolicy.Skip)
                {
                    DevLog.Log("[PlayerAttackController] 通常攻撃のダメージ適用をスキップします。");

                    // キルコードが対象へ発動したため、通常攻撃のダメージを飛ばしても命中として扱い、コンボを途切れさせない。
                    hasHit = true;
                }
                else
                {
                    hasHit = ApplyHit(attackDefinition, pendingHitEffects, isJustHit);
                    shouldApplyPendingHits = true;
                }
            }

            // 2発目以降は対象の有無に関わらず予約し、毎フレーム更新で消化する。
            // 発射演出は毎回出し、ダメージは1発目が命中処理を行った場合だけ適用する。
            int remainingHits = attackDefinition.HitCount - 1;
            if (remainingHits > 0)
            {
                _pendingAttackDefinition = attackDefinition;
                _pendingHitEffects = pendingHitEffects;
                _pendingHitCount = remainingHits;
                _pendingHitTimer = attackDefinition.HitInterval;
                _pendingIsJustHit = isJustHit;
                _shouldApplyPendingHits = shouldApplyPendingHits;
            }

            OnAttackExecuted?.Invoke(attackDefinition.AttackName, hasHit);

            return true;
        }

        /// <summary>
        ///     攻撃硬直を開始する。拍数を現在の1拍の長さで秒数に直すため、BPMが変わっても拍に合う。
        /// </summary>
        /// <param name="recoveryBeats"> 攻撃硬直の拍数。 </param>
        public void StartAttackInterval(AttackRecoveryBeats recoveryBeats)
        {
            CurrentRecoveryBeats = recoveryBeats.Value;
            _attackIntervalEvaluator.EvaluateInterval(recoveryBeats.ToInterval(_musicSyncState.BeatLength));
        }

        /// <summary> 回避後の攻撃クールダウンを開始する。 </summary>
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
        ///     保留中の多段ヒットの状態を初期化する。
        ///     ゲームプレイ停止時にも呼び、残りヒットを次のプレイへ持ち越さないようにする。
        /// </summary>
        public void ClearPendingHits()
        {
            _pendingHitCount = 0;
            _pendingHitTimer = 0d;
            _pendingAttackDefinition = null;
            _pendingHitEffects = Array.Empty<IAttackHitEffect>();
            _pendingIsJustHit = false;
            _shouldApplyPendingHits = false;
        }

        /// <summary>
        ///     保留中の多段ヒットを消化する。
        ///     ヒット間隔ごとに発射を通知し、命中処理が有効な場合だけダメージを適用する。
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

            OnFollowUpShotFired?.Invoke();

            if (_shouldApplyPendingHits)
            {
                ApplyHit(definition, _pendingHitEffects, _pendingIsJustHit);
            }

            if (_pendingHitCount <= 0)
            {
                ClearPendingHits();
            }
        }

        /// <summary>
        ///     攻撃範囲内の対象を解決し、命中対象の一覧を作る。
        /// </summary>
        /// <param name="attackDefinition"> 攻撃定義。 </param>
        /// <param name="hasLockOnTarget"> ロックオン対象がいる場合はtrue。 </param>
        /// <returns> 命中対象が1体以上いる場合はtrue。 </returns>
        private bool TryResolveHitTargets(AttackDefinition attackDefinition, bool hasLockOnTarget)
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

            // 射程外にもダメージ減衰付きで命中させるため、探索距離は射程ではなく上限距離を使う。
            // 減衰率が0でもヒット自体は成立させる。当たらないとプレイヤーには不具合に見えるため、
            // ダメージ0の表示を出して「当たったが効いていない」ことを伝える。
            float queryDistance = OUT_OF_RANGE_QUERY_DISTANCE;

            // ロックオン対象がいない場合は正面へ射程まで撃ち、途中の敵に当てる。
            // 狙っていない遠くの敵に当たらないよう、射程外の減衰ヒットは扱わない。
            if (!hasLockOnTarget)
            {
                direction = _playerTransform.forward;
                queryDistance = attackDefinition.Range;
            }

            _targetAreaQuery.QueryFanArea(
                origin,
                direction,
                queryDistance,
                attackDefinition.HalfAngleDegrees,
                _hitTargets);

            return _hitTargets.Count > 0;
        }

        /// <summary>
        ///     命中対象へ1ヒット分のダメージを適用する。
        ///     単体攻撃の場合は生存している最も近い1体のみを対象とする。
        ///     障害物の裏にいる対象には当たらない。ロックオンは外さず、弾だけが遮られる。
        /// </summary>
        /// <param name="attackDefinition"> 攻撃定義。 </param>
        /// <param name="pendingHitEffects"> 保留中の多段ヒットのエフェクト。 </param>
        /// <param name="isJustHit"> ジャスト入力による攻撃かどうかを示すフラグ。 </param>
        /// <returns> 1体以上に命中した場合はtrue。 </returns>
        private bool ApplyHit(
            AttackDefinition attackDefinition,
            IReadOnlyList<IAttackHitEffect> pendingHitEffects,
            bool isJustHit)
        {
            _attackTargets.Clear();

            // 遮蔽はヒットごとに確かめる。多段ヒットの途中で対象が障害物の裏へ入ったら、その後のヒットは当たらない。
            Vector3 origin = _playerTransform.position;

            // 一覧は水平距離の昇順。多段ヒットの途中で対象が倒れた場合は次に近い対象へ移る。
            for (int i = 0; i < _hitTargets.Count; i++)
            {
                TargetAreaHit hit = _hitTargets[i];
                if (!IsHitTargetAlive(hit))
                {
                    continue;
                }

                if (_attackOcclusion.IsOccluded(origin, hit.Target.Position))
                {
                    continue;
                }

                bool isOutOfRange = hit.Distance > attackDefinition.Range;
                _attackTargets.Add(new AttackTarget(hit.Entity, isOutOfRange));

                if (!attackDefinition.IsMultiTarget)
                {
                    break;
                }
            }

            if (_attackTargets.Count == 0)
            {
                return false;
            }

            AttackExecutor.Execute(
                attackDefinition,
                _battleState.Attacker,
                _attackTargets,
                isJustHit,
                _battleState.Attacker.BaseDamage,
                _hitResults,
                pendingHitEffects,
                notifyNormalDamage: true);

            for (int i = 0; i < _hitResults.Count; i++)
            {
                AttackResult result = _hitResults[i];
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
        ///     射程外の対象まで探索する際の最大距離（メートル）。
        ///     射程外にも減衰付きで命中させる仕様のため、実質的な上限として使う。
        /// </summary>
        private const float OUT_OF_RANGE_QUERY_DISTANCE = 1000f;

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

        /// <summary>
        ///     拍子に対応する攻撃定義を取得する。
        /// </summary>
        /// <param name="beatType"> 拍子。 </param>
        /// <returns> 攻撃定義。見つからない場合はnull。 </returns>
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
        private readonly PlayerAttackPresenter _attackPresenter;
        private readonly PlayerBattleState _battleState;
        private readonly PlayerActionRestrictionState _actionRestrictionState;
        private readonly SkillController _skillController;
        private readonly TargetSystemController _targetingSystem;
        private readonly AttackIntervalEvaluator _attackIntervalEvaluator;
        private readonly IMusicSyncService _musicSyncService;
        private readonly MusicSyncState _musicSyncState;
        private readonly TargetAreaQuery _targetAreaQuery;
        private readonly Transform _playerTransform;
        private readonly PendingAttackEffectService _pendingAttackEffectService;
        private readonly IAttackOcclusionViewModel _attackOcclusion;
        private readonly AttackRecoveryBeats _attackRecoveryBeats;
        private readonly List<TargetAreaHit> _hitTargets = new List<TargetAreaHit>();
        private readonly List<AttackTarget> _attackTargets = new List<AttackTarget>();
        private readonly List<AttackResult> _hitResults = new List<AttackResult>();
        private IReadOnlyList<IAttackHitEffect> _pendingHitEffects = Array.Empty<IAttackHitEffect>();
        private double _attackCooldownRemainig;
        private double _attackCooldown;
        private AttackDefinition _pendingAttackDefinition;
        private int _pendingHitCount;
        private double _pendingHitTimer;
        private bool _pendingIsJustHit;
        private bool _shouldApplyPendingHits;
    }
}
