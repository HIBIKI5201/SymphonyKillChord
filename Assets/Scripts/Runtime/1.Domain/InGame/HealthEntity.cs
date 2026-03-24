using UnityEngine;

namespace KillChord.Runtime.Domain
{
    public class HealthEntity
    {
        public HealthEntity(float health)
        {
            CurrentHealth = new (health);
            MaxHealth = new (health);
        }
		
        public Health CurrentHealth;
        public readonly Health MaxHealth;
    }
}
