using KillChord.Runtime.Adaptor.InGame.UI;
using R3;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.UI
{
    /// <summary>
    ///     HPのHUD用のViewModelクラス。
    /// </summary>
    public class HealthHudViewModel : IHealthHudViewModel
    {
        public HealthHudViewModel(float currentHealth, float maxHealth)
        {
            _healthHudDto = new ReactiveProperty<HealthHudDTO>(new HealthHudDTO(currentHealth, maxHealth));
        }
        
        /// <summary> HP情報を保持するReactiveProperty。 </summary>
        public ReadOnlyReactiveProperty<HealthHudDTO> HealthHudDTO => _healthHudDto;

        /// <summary>
        ///     HP情報を更新する。
        /// </summary>
        /// <param name="dto"></param>
        public void UpdateHealth(in HealthHudDTO dto)
        {
            _healthHudDto.Value = dto;
        }

        private ReactiveProperty<HealthHudDTO> _healthHudDto;
    }
}
