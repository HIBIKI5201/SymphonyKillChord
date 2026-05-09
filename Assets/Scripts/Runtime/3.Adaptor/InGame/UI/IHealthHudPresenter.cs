using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.UI
{
    /// <summary>
    ///     HPをHUDに反映するPresenter。
    /// </summary>
    public interface IHealthHudPresenter
    {
        /// <summary>
        ///     HP HUDを更新する処理。
        /// </summary>
        /// <param name="health">値更新後のHealthEntity</param>
        /// <param name="amountChanged">HPの変化量</param>
        public void UpdateHealthHud(HealthEntity health, float amountChanged);
    }
}
