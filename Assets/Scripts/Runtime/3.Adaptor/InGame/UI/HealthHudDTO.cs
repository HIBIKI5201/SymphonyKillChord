namespace KillChord.Runtime.Adaptor.InGame.UI
{
    /// <summary>
    ///     HP HUD用のHP情報DTO。
    /// </summary>
    public readonly struct HealthHudDTO
    {
        public HealthHudDTO(float currentHealth, float maxHealth)
        {
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
        }

        /// <summary> 現在HP </summary>
        public readonly float CurrentHealth;
        /// <summary> 最大HP </summary>
        public readonly float MaxHealth;
    }
}
