using System;

namespace KillChord.Runtime.Adaptor.InGame.UI
{
    /// <summary>
    ///     HPをHUDに反映するPresenter。
    /// </summary>
    public interface IHealthHudPresenter : IDisposable
    {
        /// <summary>
        ///     HP HUDを更新する処理。
        /// </summary>
        /// <param name="currentHealth">現在HP</param>
        /// <param name="maxHealth">最大HP</param>
        /// <param name="amountChanged">HPの変化量</param>
        public void UpdateHealthHud(float currentHealth, float maxHealth, float amountChanged);
        /// <summary>
        ///     有効化処理。
        /// </summary>
        public void Activate();
        /// <summary>
        ///     無効化処理。
        /// </summary>
        public void Deactivate();
    }
}
