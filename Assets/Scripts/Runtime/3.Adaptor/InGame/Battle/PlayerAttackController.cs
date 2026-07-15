using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Utility.Persistent;
using System;
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
        public PlayerAttackController(
            AttackResultPresenter presenter,
            PlayerBattleState battleState,
            SkillController skillController,
            TargetSystemController targetingSystem,
            AttackIntervalEvaluator attackIntervalEvaluator,
            IMusicSyncService musicSyncService,
            MusicSyncState musicSyncState,
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
            AttackRotationSpeed = attackRotationSpeed;
            _baseDamage = baseDamage;

            _attackCooldown = attackCooldown * (60d / musicSyncState.Bpm);
            _attackCooldownRemainig = 0d;
        }

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
                Debug.LogError("TargetingSystemが設定されていません。");
                return false;
            }

            bool hasTarget = TryUpdateCurrentTarget();

            float now = Time.unscaledTime;
            BeatType beatType = _musicSyncService.GetCurrentBeatType(now);

            _skillController.TryExecuteSkill(BattleActionType.Attack, beatType, now);

            AttackDefinition attackDefinition = GetDifinitionByBeatType(beatType);   //攻撃定義未発見時にnullが返る

            if (attackDefinition == null) return false;

            StartAttackInterval();
            StartAttackCooldown();

            if (!hasTarget)
            {
                resultBeatType = (int)beatType;
                return true;
            }

            CharacterEntity targetEntity = _battleState.Target as CharacterEntity;
            if (targetEntity == null)
            {
                resultBeatType = (int)beatType;
                return true;
            }

            BuffContext buffContext = new BuffContext(_battleState.Attacker, _battleState.Target as CharacterEntity);
            _ = _battleState.Attacker.BuffSystem.Execute(buffContext, BuffExecuteTiming.Attack_Logic_Before);
            // TODO 射線判定などを追加して、攻撃がヒットするかどうかを判定する必要がある。
            AttackResult result = AttackExecutor.Execute(attackDefinition,
                _battleState.Attacker,
                _battleState.Target,
                false,
                _battleState.Attacker.BaseDamage
            );

            BuffContext buffContextPost = new BuffContext(_battleState.Attacker.BuffSystem.Execute(new BuffContext(buffContext.Attacker, buffContext.Target, result), BuffExecuteTiming.Attack_Logic_After));


            // TODO 攻撃対象を特定するための、一時的な手段としてEntityのHashCodeを使う
            Debug.Log($"[PlayerAttackController]攻撃対象のId：{targetEntity.Id}");
            EventBus<EOnTakeDamage>.Raise(new EOnTakeDamage(buffContextPost.AttackResult.FinalDamage.Value, buffContextPost.AttackResult.IsCritical,
                targetEntity.Id));

            buffContext.Attacker.SetDamage(buffContextPost.AttackResult.FinalDamage);
            _presenter.Push(buffContextPost.AttackResult);

            resultBeatType = (int)beatType;
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

        /// <summary> 毎フレームクールダウンを減算する。 </summary>
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
        }

        /// <summary>
        ///     現在のターゲット状態を更新します。
        /// </summary>
        /// <returns> 攻撃対象が存在する場合はtrueです。 </returns>
        private bool TryUpdateCurrentTarget()
        {
            if (!_targetingSystem.TryGetCurrentTargetEntity(out CharacterEntity targetEntity))
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
        private readonly int _baseDamage;
        private double _attackCooldownRemainig;
        private double _attackCooldown;
    }
}
