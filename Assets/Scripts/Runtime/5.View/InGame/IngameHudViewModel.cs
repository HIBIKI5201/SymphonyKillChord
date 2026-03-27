using KillChord.Runtime.Adaptor;
using R3;

namespace KillChord.Runtime.View
{
    public class IngameHudViewModel : IIngameHudViewModel
    {
        public ReadOnlyReactiveProperty<float> HealthRate => _healthRate;

        private ReactiveProperty<float> _healthRate = new(1);

        public void UpdateHealth(in IngameHudDTO dto)
        {
            if (dto.MaxHealth <= 0)
            {
                _healthRate.Value = 0;
                return;
            }

            _healthRate.Value = dto.CurrentHealth / dto.MaxHealth;
        }
    }
}