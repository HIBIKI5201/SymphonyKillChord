using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.UI
{
    public readonly ref struct HUDEnemyHealthDTO
    {
        public HUDEnemyHealthDTO(float currentHealth, float maxHealth, bool isLockon, in Vector3 targetPosition)
        {
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            IsLockon = isLockon;
            TargetPosition = targetPosition;
        }

        public readonly Vector3 TargetPosition;
        public readonly float CurrentHealth;
        public readonly float MaxHealth;
        public readonly bool IsLockon;
    }
}
