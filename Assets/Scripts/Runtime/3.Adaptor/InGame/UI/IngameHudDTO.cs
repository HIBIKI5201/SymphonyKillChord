using System.Numerics;

namespace KillChord.Runtime.Adaptor.InGame.UI
{
    /// <summary>
    ///     HUDの表示更新に必要なデータを保持するデータ転送用構造体。
    /// </summary>
    public readonly ref struct IngameHudDTO
    {
        /// <summary>
        ///     最大 HP と現在 HP を指定して生成する。
        /// </summary>
        public IngameHudDTO(float maxHealth, float currentHealth)
        {
            MaxHealth = maxHealth;
            CurrentHealth = currentHealth;
        }

        public readonly float MaxHealth;
        public readonly float CurrentHealth;
    }
}