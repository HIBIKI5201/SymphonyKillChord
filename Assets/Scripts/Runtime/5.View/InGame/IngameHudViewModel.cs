using KillChord.Runtime.Adaptor;
using R3;

namespace KillChord.Runtime.View
{
    public class IngameHudViewModel : IIngameHudViewModel
    {
        public ReactiveProperty<float> HealthRate { get; } = new();

        public void UpdateHealth(in IngameHudDTO dto)
        {
            if (dto.MaxHealth <= 0)
            {
                HealthRate.Value = 0;
                return;
            }

            HealthRate.Value = dto.CurrentHealth / dto.MaxHealth;
        }
    }
}