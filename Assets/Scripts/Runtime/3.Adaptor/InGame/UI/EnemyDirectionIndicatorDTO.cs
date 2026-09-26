using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.UI
{
    /// <summary>
    ///     敵方向表示の1スロット分の表示情報を保持する。
    /// </summary>
    public readonly ref struct EnemyDirectionIndicatorDTO
    {
        /// <summary>
        ///     敵方向表示の表示情報を生成する。
        /// </summary>
        /// <param name="slotIndex"> 更新対象の表示スロット番号。 </param>
        /// <param name="isVisible"> 表示する場合はtrue。 </param>
        /// <param name="direction"> プレイヤーから敵への水平方向。 </param>
        public EnemyDirectionIndicatorDTO(int slotIndex, bool isVisible, in Vector3 direction)
        {
            SlotIndex = slotIndex;
            IsVisible = isVisible;
            Direction = direction;
        }

        /// <summary> 更新対象の表示スロット番号。 </summary>
        public int SlotIndex { get; }

        /// <summary> 表示する場合はtrue。 </summary>
        public bool IsVisible { get; }

        /// <summary> プレイヤーから敵への水平方向。 </summary>
        public Vector3 Direction { get; }
    }
}
