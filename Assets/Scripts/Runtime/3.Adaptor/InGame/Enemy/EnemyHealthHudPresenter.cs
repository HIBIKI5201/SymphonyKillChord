using KillChord.Runtime.Adaptor.InGame.UI;
using KillChord.Runtime.Domain.InGame.Battle;
using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
     /// <summary>
     ///     プレイヤーのHPをHUDに反映するPresenter。
     /// </summary>
    public class EnemyHealthHudPresenter : IHealthHudPresenter
    {
        public EnemyHealthHudPresenter(IDefender entity, IHealthHudViewModel healthHudViewModel)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity), "敵EntityがNULL。");
            _entity = entity;
            // TODO 敵HPのViewModel実装待ち
            //if (healthHudViewModel == null) throw new ArgumentNullException(nameof(healthHudViewModel), "敵HPのViewModelがNULL。");
            //_healthHudViewModel = healthHudViewModel;

            _entity.OnHealthChanged += UpdateHealthHud;
        }
        public void Dispose()
        {
            _entity.OnHealthChanged -= UpdateHealthHud;
        }

        /// <summary>
        ///     有効化処理。
        /// </summary>
        public void Activate()
        {
            _entity.OnHealthChanged += UpdateHealthHud;
        }
        /// <summary>
        ///     無効化処理。
        /// </summary>
        public void Deactivate()
        {
            _entity.OnHealthChanged -= UpdateHealthHud;
        }

        /// <summary>
        ///     HP HUDを更新する処理。
        /// </summary>
        /// <param name="currentHealth">現在HP</param>
        /// <param name="maxHealth">最大HP</param>
        /// <param name="amountChanged">HPの変化量</param>
        public void UpdateHealthHud(float currentHealth, float maxHealth, float amountChanged)
        {
            // TODO 敵のHP表現は実装待ち
            Debug.Log($"[EnemyHealthHudPresenter] 敵HP更新：{currentHealth} / {maxHealth}　変化量：{amountChanged}");
        }

        private IDefender _entity;
        private IHealthHudViewModel _healthHudViewModel;
    }
}
