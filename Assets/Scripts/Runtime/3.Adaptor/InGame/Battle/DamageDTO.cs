using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Adaptor
{
    public readonly ref struct DamageDTO
    {
        public DamageDTO(Health max, Health current)
        {
            MaxHealth = max;
            CurrentHealth = current;
        }

        public readonly Health MaxHealth;
        public readonly Health CurrentHealth;
    }
}
