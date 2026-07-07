namespace KillChord.Runtime.Adaptor.InGame.UI
{
    public readonly ref struct HUDEnemyHealthDTO
    {
        public HUDEnemyHealthDTO(float currentHealth, float maxHealth, bool isLockon)
        {
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            IsLockon = isLockon;
        }

        public readonly float CurrentHealth;
        public readonly float MaxHealth;
        public readonly bool IsLockon;
    }
}
