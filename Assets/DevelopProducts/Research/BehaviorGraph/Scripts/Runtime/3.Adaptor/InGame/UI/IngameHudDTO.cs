using System.Numerics;

namespace DevelopProducts.BehaviorGraph.Runtime.Adaptor
{
    /// <summary>
    ///     HUDの表示更新に必要なデータを保持するデータ転送用構造体。
    /// </summary>
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