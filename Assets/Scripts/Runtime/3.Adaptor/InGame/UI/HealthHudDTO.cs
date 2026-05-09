using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.UI
{
    /// <summary>
    ///     HP HUD用のHP情報DTO。
    /// </summary>
    public readonly struct HealthHudDTO
    {
        public HealthHudDTO(float maxHealth, float currentHealth)
        {
            MaxHealth = maxHealth;
            CurrentHealth = currentHealth;
        }

        /// <summary> 最大HP </summary>
        public readonly float MaxHealth;
        /// <summary> 現在HP </summary>
        public readonly float CurrentHealth;
    }
}
