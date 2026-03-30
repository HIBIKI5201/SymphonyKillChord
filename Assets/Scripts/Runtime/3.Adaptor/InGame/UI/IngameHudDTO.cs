using System.Numerics;

namespace KillChord.Runtime.Adaptor
{
    public readonly ref struct IngameHudDTO
    {
        public IngameHudDTO(float maxHealth, float currentHealth)
        {
            MaxHealth = maxHealth;
            CurrentHealth = currentHealth;
        }

        public readonly float MaxHealth;
        public readonly float CurrentHealth;
    }
}