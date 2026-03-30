using KillChord.Runtime.Domain.InGame.Character;

namespace KillChord.Runtime.Adaptor.InGame.Battle
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
