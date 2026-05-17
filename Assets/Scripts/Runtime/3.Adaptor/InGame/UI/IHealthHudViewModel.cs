using R3;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.UI
{
    /// <summary>
    ///     HPのHUD用のViewModelインタフェース。
    /// </summary>
    public interface IHealthHudViewModel
    {
        public ReadOnlyReactiveProperty<HealthHudDTO> HealthHudDTO { get; }
        /// <summary>
        ///     HP情報を更新する。
        /// </summary>
        /// <param name="dto"></param>
        public void UpdateHealth(in HealthHudDTO dto);
    }
}
