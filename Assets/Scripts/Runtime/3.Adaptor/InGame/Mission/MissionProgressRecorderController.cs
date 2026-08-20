using KillChord.Runtime.Adaptor.InGame.Battle;
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
        public MissionProgressRecorderController(MissionProgress missionProgress)
        {
            _missionProgress = missionProgress
                ?? throw new ArgumentNullException(nameof(missionProgress));
        }

        /// <summary>
        ///     プレイヤーと攻撃Controllerのイベントを購読します。
        /// </summary>
        /// <param name="playerEntity"> プレイヤーEntityです。 </param>
        /// <param name="attackController"> プレイヤー攻撃Controllerです。 </param>
        public void Bind(
            CharacterEntity playerEntity,
            PlayerAttackController attackController)
        {
            Unbind();

            _playerEntity = playerEntity
                ?? throw new ArgumentNullException(nameof(playerEntity));
            _attackController = attackController
                ?? throw new ArgumentNullException(nameof(attackController));

            _playerEntity.OnHealthChanged += HandleHealthChanged;
            _attackController.OnAttackExecuted += HandleAttackExecuted;
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
            }

            _playerEntity = null;
            _attackController = null;
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
        private CharacterEntity _playerEntity;
        private PlayerAttackController _attackController;
        private int _currentCombo;

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

            if (!hasHit)
            {
                _currentCombo = 0;
                return;
            }

            _currentCombo++;
            _missionProgress.RecordCombo(_currentCombo);
        }
    }
}
