using KillChord.Runtime.Adaptor.InGame.UI;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Utility.Persistent;
using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     敵のHPをHUDに反映するPresenter。
    /// </summary>
    public class EnemyHealthHudPresenter : IHealthHudPresenter
    {
        public EnemyHealthHudPresenter(
            IDefender entity,
            Guid defenderId,
            IHealthHudViewModel healthHudViewModel,
            IDamageNumber damageNumberView
            )
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity), "敵EntityがNULL。");
            _entity = entity;
            if (healthHudViewModel == null) throw new ArgumentNullException(nameof(healthHudViewModel), "敵HPのViewModelがNULL。");
            _healthHudViewModel = healthHudViewModel;
            if (damageNumberView == null) throw new ArgumentNullException(nameof(damageNumberView), "敵ダメージ表示ViewがNULL。");
            _damageNumberView = damageNumberView;

            _defenderId = defenderId;
        }
        public void Dispose()
        {
            Deactivate();
        }

        /// <summary>
        ///     有効化処理。
        /// </summary>
        public void Activate()
        {
            if (_isActive) return;
            _entity.OnHealthChanged += UpdateHealthHud;
            EventBus<EOnTakeDamage>.Register(HandleTakeDamage);
            _isActive = true;
        }
        /// <summary>
        ///     無効化処理。
        /// </summary>
        public void Deactivate()
        {
            if (!_isActive) return;
            _entity.OnHealthChanged -= UpdateHealthHud;
            EventBus<EOnTakeDamage>.Unregister(HandleTakeDamage);
            _isActive = false;
        }

        /// <summary>
        ///     HP HUDを更新する処理。
        /// </summary>
        /// <param name="currentHealth">現在HP</param>
        /// <param name="maxHealth">最大HP</param>
        /// <param name="amountChanged">HPの変化量</param>
        public void UpdateHealthHud(float currentHealth, float maxHealth, float amountChanged)
        {
            _healthHudViewModel.UpdateHealth(new HealthHudDTO(currentHealth, maxHealth));

            Debug.Log($"[EnemyHealthHudPresenter] 敵HP更新：{currentHealth} / {maxHealth}　変化量：{amountChanged}");
        }

        private IDefender _entity;
        private Guid _defenderId;
        private IHealthHudViewModel _healthHudViewModel;
        private IDamageNumber _damageNumberView;

        private bool _isActive = false;

        /// <summary>
        ///     ダメージを受けた際の処理。
        /// </summary>
        /// <param name="damageEvent">ダメージ情報</param>
        private void HandleTakeDamage(EOnTakeDamage damageEvent)
        {
            if (damageEvent.DefenderId != _defenderId || damageEvent.Damage <= 0)
            {
                return;
            }

            DamageNumberType type = GetDamageNumberType(damageEvent);

            _damageNumberView.ShowDamage(new DamageNumberDTO(damageEvent.Damage, type));
        }

        /// <summary>
        ///     ダメージ情報から表示種類を決定する。
        /// </summary>
        /// <param name="eventData">ダメージ情報</param>
        /// <returns>表示するダメージ番号の種類</returns>
        private static DamageNumberType GetDamageNumberType(
            EOnTakeDamage eventData)
        {
            if (eventData.Critical)
            {
                return DamageNumberType.Critical;
            }

            if (eventData.AttackType == DamageAttackType.Skill)
            {
                return DamageNumberType.Skill;
            }

            return DamageNumberType.Normal;
        }
    }
}
