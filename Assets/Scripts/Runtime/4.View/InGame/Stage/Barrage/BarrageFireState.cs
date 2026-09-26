using Unity.Entities;

namespace KillChord.Runtime.View.InGame.Stage.Barrage
{
    /// <summary>
    ///     タレットが発射中に保持する状態です。
    /// </summary>
    /// <remarks>
    ///     Baker時に無効状態で付与し、開始・停止は有効フラグの切り替えだけで行います。
    ///     Add/Removeによる構造変化を避けるためにIEnableableComponentとしています。
    /// </remarks>
    public struct BarrageFireState : IComponentData, IEnableableComponent
    {
        /// <summary> 停止命令があるまで撃ち続けることを表す残弾数です。 </summary>
        public const int INFINITE_SHOTS = -1;

        /// <summary> 残りの発射回数です。<see cref="INFINITE_SHOTS"/>の場合は無制限です。 </summary>
        public int RemainingShots;

        /// <summary> 次の発射までの残り秒数です。 </summary>
        public float Timer;
    }
}
