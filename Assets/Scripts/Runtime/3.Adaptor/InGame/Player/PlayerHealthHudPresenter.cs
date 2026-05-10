using KillChord.Runtime.Adaptor.InGame.UI;
using KillChord.Runtime.Domain.InGame.Battle;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Player
{
    /// <summary>
    ///     プレイヤーのHPをHUDに反映するPresenter。
    /// </summary>
    public class PlayerHealthHudPresenter : IHealthHudPresenter, IDisposable
    {
        public PlayerHealthHudPresenter(IDefender entity, IHealthHudViewModel healthHudViewModel)
        {
            _entity = entity;
            _healthHudViewModel = healthHudViewModel;

            _entity.OnHealthChanged += UpdateHealthHud;
        }

        public void Dispose()
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
            // TODO amountChangedを使ってダメージ表示など
            _healthHudViewModel.UpdateHealth(new HealthHudDTO(currentHealth, maxHealth));

        }

        private IDefender _entity;
        private IHealthHudViewModel _healthHudViewModel;
    }
}
