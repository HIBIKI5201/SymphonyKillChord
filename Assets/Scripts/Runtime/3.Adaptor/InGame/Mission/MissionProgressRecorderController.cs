using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Mission;
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
        /// <param name="attackController"> プレイヤー攻撃Controllerです。 </param>
        /// <param name="skillController"> スキルControllerです。 </param>
        public void Bind(
            CharacterEntity playerEntity,
            PlayerAttackController attackController,
            SkillController skillController)
        {
            Unbind();

            _playerEntity = playerEntity
                ?? throw new ArgumentNullException(nameof(playerEntity));
            _attackController = attackController
                ?? throw new ArgumentNullException(nameof(attackController));
            _skillController = skillController
                ?? throw new ArgumentNullException(nameof(skillController));

            _playerEntity.OnHealthChanged += HandleHealthChanged;
            _playerEntity.OnDamageAvoided += HandleDamageAvoided;
            _attackController.OnAttackExecuted += HandleAttackExecuted;
            _skillController.OnSkillAnimationRequested += HandleSkillAnimationRequested;
        }

        /// <summary>
        ///     購読中の戦闘イベントを解除します。
        /// </summary>
        public void Unbind()
        {
            if (_playerEntity != null)
            {
                _playerEntity.OnHealthChanged -= HandleHealthChanged;
                _playerEntity.OnDamageAvoided -= HandleDamageAvoided;
            }

            if (_attackController != null)
            {
                _attackController.OnAttackExecuted -= HandleAttackExecuted;
            }

            if (_skillController != null)
            {
                _skillController.OnSkillAnimationRequested -= HandleSkillAnimationRequested;
            }

            _playerEntity = null;
            _attackController = null;
            _skillController = null;
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
        private int _currentCombo;

        /// <summary>
        ///     実際にダメージを回避したことをミッションへ通知します。
        /// </summary>
        /// <param name="damage"> 回避したダメージです。 </param>
        private void HandleDamageAvoided(Damage damage)
        {
            _missionEventController.NotifyActionPerformed(MissionActionKind.Evade);
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
