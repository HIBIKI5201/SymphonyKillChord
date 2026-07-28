using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.UI
{
    /// <summary>
    ///     ロックオンHUDへ渡す表示情報を保持する。
    /// </summary>
    public readonly ref struct HUDEnemyHealthDTO
    {
        /// <summary>
        ///     表示情報を生成する。
        /// </summary>
        /// <param name="currentHealth"> 現在体力。 </param>
        /// <param name="maxHealth"> 最大体力。 </param>
        /// <param name="displayState"> HUDの表示状態。 </param>
        /// <param name="targetPosition"> 表示対象のワールド座標。 </param>
        public HUDEnemyHealthDTO(
            float currentHealth,
            float maxHealth,
            LockOnDisplayState displayState,
            in Vector3 targetPosition)
        {
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            DisplayState = displayState;
            TargetPosition = targetPosition;
        }

        /// <summary> 表示対象のワールド座標。 </summary>
        public readonly Vector3 TargetPosition;

        /// <summary> 現在体力。 </summary>
        public readonly float CurrentHealth;

        /// <summary> 最大体力。 </summary>
        public readonly float MaxHealth;

        /// <summary> HUDの表示状態。 </summary>
        public readonly LockOnDisplayState DisplayState;
    }
}
