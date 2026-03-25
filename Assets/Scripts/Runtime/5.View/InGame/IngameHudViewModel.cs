using KillChord.Runtime.Adaptor;
using R3;

namespace KillChord.Runtime.View
{
    public class IngameHudViewModel : IIngameHudViewModel
    {
        public ReactiveProperty<float> HealthRate { get; } = new();

        public void UpdateHealth(in IngameHudDTO dto)
        {
            HealthRate.Value = dto.HealthRate;
        }
    }
}