using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    public readonly struct IngameHudDTO
    {
        public IngameHudDTO(float healthRate)
        {
            HealthRate = healthRate;
        }

        public readonly float HealthRate;
    }
}