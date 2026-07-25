using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Adaptor.InGame.Player;
using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Music;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     戦闘モジュールのイベントをミッション実績値へ記録します。
    /// </summary>
    public sealed class MissionProgressRecorderController : IDisposable
    {
        /// <summary>
        ///     記録対象のミッション進行を設定します。
        /// </summary>
        /// <param name="missionProgress"> 記録対象です。 </param>
        /// <param name="missionEventController"> ミッション行動を通知するControllerです。 </param>
        public MissionProgressRecorderController(
            MissionProgress missionProgress,
            MissionEventController missionEventController)
        {
            _missionProgress = missionProgress
                ?? throw new ArgumentNullException(nameof(missionProgress));
            _missionEventController = missionEventController
                ?? throw new ArgumentNullException(nameof(missionEventController));
        }

        /// <summary>
        ///     プレイヤー、攻撃、スキルのイベントを購読します。
        /// </summary>
        /// <param name="playerEntity"> プレイヤーEntityです。 </param>
        /// <param name="playerController"> プレイヤー移動Controllerです。 </param>
        /// <param name="attackController"> プレイヤー攻撃Controllerです。 </param>
        /// <param name="skillController"> スキルControllerです。 </param>
        /// <param name="targetSystemController"> ターゲット選択Controllerです。 </param>
        public void Bind(
            CharacterEntity playerEntity,
            PlayerController playerController,
            PlayerAttackController attackController,
            SkillController skillController,
            TargetSystemController targetSystemController)
        {
            Unbind();

            _playerEntity = playerEntity
                ?? throw new ArgumentNullException(nameof(playerEntity));
            _attackController = attackController
                ?? throw new ArgumentNullException(nameof(attackController));
            _skillController = skillController
                ?? throw new ArgumentNullException(nameof(skillController));
            _playerController = playerController
                ?? throw new ArgumentNullException(nameof(playerController));
            _targetSystemController = targetSystemController
                ?? throw new ArgumentNullException(nameof(targetSystemController));

            _playerEntity.OnHealthChanged += HandleHealthChanged;
            _playerController.OnMoved += HandleMoved;
            _playerController.OnDodgeSucceeded += HandleDodgeSucceeded;
            _attackController.OnAttackExecuted += HandleAttackExecuted;
            _attackController.OnAttackBeatExecuted += HandleAttackBeatExecuted;
            _skillController.OnSkillAnimationRequested += HandleSkillAnimationRequested;
            _targetSystemController.OnTargetLocked += HandleTargetLocked;
        }

        /// <summary>
        ///     購読中の戦闘イベントを解除します。
        /// </summary>
        public void Unbind()
        {
            if (_playerEntity != null)
            {
                _playerEntity.OnHealthChanged -= HandleHealthChanged;
            }

            if (_attackController != null)
            {
                _attackController.OnAttackExecuted -= HandleAttackExecuted;
                _attackController.OnAttackBeatExecuted -= HandleAttackBeatExecuted;
            }

            if (_playerController != null)
            {
                _playerController.OnMoved -= HandleMoved;
                _playerController.OnDodgeSucceeded -= HandleDodgeSucceeded;
            }

            if (_skillController != null)
            {
                _skillController.OnSkillAnimationRequested -= HandleSkillAnimationRequested;
            }

            if (_targetSystemController != null)
            {
                _targetSystemController.OnTargetLocked -= HandleTargetLocked;
            }

            _playerEntity = null;
            _attackController = null;
            _skillController = null;
            _playerController = null;
            _targetSystemController = null;
            _currentCombo = 0;
        }

        /// <summary>
        ///     イベント購読を解除します。
        /// </summary>
        public void Dispose()
        {
            Unbind();
        }

        private readonly MissionProgress _missionProgress;
        private readonly MissionEventController _missionEventController;
        private CharacterEntity _playerEntity;
        private PlayerAttackController _attackController;
        private SkillController _skillController;
        private PlayerController _playerController;
        private TargetSystemController _targetSystemController;
        private int _currentCombo;

        /// <summary>
        ///     回避の実行をミッションへ通知します。敵の攻撃を実際に避けられたかどうかは問いません。
        /// </summary>
        private void HandleDodgeSucceeded()
        {
            _missionEventController.NotifyActionPerformed(MissionActionKind.Dodge);
        }

        /// <summary> プレイヤー移動をMissionへ通知します。 </summary>
        private void HandleMoved()
        {
            _missionEventController.NotifyActionPerformed(MissionActionKind.Move);
        }

        /// <summary> ロックオン成立をMissionへ通知します。 </summary>
        private void HandleTargetLocked()
        {
            _missionEventController.NotifyActionPerformed(MissionActionKind.LockOn);
        }

        /// <summary>
        ///     攻撃拍子をMissionへ通知します。
        /// </summary>
        /// <param name="beatType"> 実行した攻撃の拍子です。 </param>
        private void HandleAttackBeatExecuted(BeatType beatType)
        {
            MissionActionKind actionKind = beatType switch
            {
                BeatType.One => MissionActionKind.AttackOneBeat,
                BeatType.Two => MissionActionKind.AttackTwoBeat,
                BeatType.Three => MissionActionKind.AttackThreeBeat,
                BeatType.Four => MissionActionKind.AttackFourBeat,
                BeatType.Six => MissionActionKind.AttackSixBeat,
                BeatType.Eight => MissionActionKind.AttackEightBeat,
                _ => MissionActionKind.Attack,
            };
            _missionEventController.NotifyActionPerformed(actionKind);
        }

        /// <summary>
        ///     プレイヤーHP変化を記録します。
        /// </summary>
        /// <param name="currentHealth"> 現在HPです。 </param>
        /// <param name="maximumHealth"> 最大HPです。 </param>
        /// <param name="amountChanged"> HP変化量です。 </param>
        private void HandleHealthChanged(
            float currentHealth,
            float maximumHealth,
            float amountChanged)
        {
            if (amountChanged >= 0f)
            {
                return;
            }

            _missionProgress.RecordDamageTaken(-amountChanged);
            _currentCombo = 0;
        }

        /// <summary>
        ///     プレイヤー攻撃の武器種類とコンボを記録します。
        /// </summary>
        /// <param name="weaponId"> 使用した武器IDです。 </param>
        /// <param name="hasHit"> 攻撃が命中した場合はtrueです。 </param>
        private void HandleAttackExecuted(string weaponId, bool hasHit)
        {
            _missionProgress.RecordWeaponUse(weaponId);
            _missionEventController.NotifyActionPerformed(MissionActionKind.Attack);

            if (!hasHit)
            {
                _currentCombo = 0;
                return;
            }

            _currentCombo++;
            _missionProgress.RecordCombo(_currentCombo);
        }

        /// <summary>
        ///     スキル発動をミッションへ通知します。
        /// </summary>
        /// <param name="animationKey"> 発動したスキルのアニメーションキーです。 </param>
        private void HandleSkillAnimationRequested(string animationKey)
        {
            _missionEventController.NotifyActionPerformed(MissionActionKind.Skill);
        }
    }
}
