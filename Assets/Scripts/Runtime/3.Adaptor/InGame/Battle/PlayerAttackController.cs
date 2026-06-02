using KillChord.Runtime.Adaptor.InGame.Camera.Target;
using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Battle;
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
        /// <param name="targetSelectorController"></param>
        /// <param name="musicSyncService"></param>
        public PlayerAttackController(
            AttackResultPresenter presenter,
            PlayerBattleState battleState,
            SkillController skillController,
            TargetSelectorController targetSelectorController,
            AttackIntervalEvaluator attackIntervalEvaluator,
            IMusicSyncService musicSyncService,
            float attackRotationSpeed
        )
        {
            _attackIntervalEvaluator = attackIntervalEvaluator;
            _presenter = presenter;
            _battleState = battleState;
            _skillController = skillController;
            _targetSelectorController = targetSelectorController;
            _musicSyncService = musicSyncService;
            AttackRotationSpeed = attackRotationSpeed;
        }

        /// <summary> 現在攻撃中かどうかを表すプロパティ。 </summary>
        public bool IsAttacking => _attackIntervalEvaluator.IsAttacking;

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
            if (_targetSelectorController == null)
            {
                Debug.LogError("TargetSelectorControllerが設定されていません。");
                return false;
            }

            if (!_targetSelectorController.TryGetCurrentTargetEntity(out var targetEntity))
            {
                Debug.Log("攻撃対象が選択されていません。");
                return false;
            }

            _battleState.ChangeTarget(targetEntity);

            // 現在の ILockOnTarget を取得して保持（View 側が参照する）
            if (_targetSelectorController.TryGetCurrentTarget(out var lockOnTarget))
            {
                HasCurrentLockOnTarget = true;
                CurrentLockOnTargetPosition = lockOnTarget.Position;
            }
            else
            {
                HasCurrentLockOnTarget = false;
                CurrentLockOnTargetPosition = Vector3.zero;
            }

            float now = Time.unscaledTime;
            BeatType beatType = _musicSyncService.GetCurrentBeatType(now);

            _skillController.CheckSkill(BattleActionType.Attack, beatType, now);

            AttackDefinition attackDefinition;
            try
            {
                attackDefinition = _battleState.Attacker.CombatSpec.GetAttackDefinitionByBeatType(beatType);
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogWarning(ex.Message);
                return false;
            }

            _attackIntervalEvaluator.EvaluateInterval();

            // TODO 射線判定などを追加して、攻撃がヒットするかどうかを判定する必要がある。
            AttackResult result = AttackExecutor.Execute(attackDefinition,
                _battleState.Attacker,
                _battleState.Target
            );

            // TODO 攻撃対象を特定するための、一時的な手段としてEntityのHashCodeを使う
            Debug.Log($"[PlayerAttackController]攻撃対象のId：{targetEntity.Id}");
            EventBus<EOnTakeDamage>.Raise(new EOnTakeDamage(result.FinalDamage.Value, result.IsCritical,
                targetEntity.Id));

            _presenter.Push(result);

            resultBeatType = (int)beatType;
            return true;
        }

        private readonly AttackResultPresenter _presenter;
        private readonly PlayerBattleState _battleState;
        private readonly SkillController _skillController;
        private readonly TargetSelectorController _targetSelectorController;
        private readonly AttackIntervalEvaluator _attackIntervalEvaluator;
        private readonly IMusicSyncService _musicSyncService;
    }
}
