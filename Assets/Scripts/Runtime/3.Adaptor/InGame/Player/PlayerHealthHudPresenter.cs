using KillChord.Runtime.Adaptor.InGame.UI;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
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

            _entity.OnDamageTaken += UpdateHealthHud;
        }

        public void Dispose()
        {
            _entity.OnDamageTaken -= UpdateHealthHud;
        }

        /// <summary>
        ///     HP HUDを更新する処理。
        /// </summary>
        /// <param name="health">値更新後のHealthEntity</param>
        /// <param name="amountChanged">HPの変化量</param>
        public void UpdateHealthHud(HealthEntity health, float amountChanged)
        {
            // TODO amountChangedを使ってダメージ表示など
            _healthHudViewModel.UpdateHealth(new HealthHudDTO(health.MaxHealth.Value, health.CurrentHealth.Value));

        }

        private IDefender _entity;
        private IHealthHudViewModel _healthHudViewModel;
    }
}
