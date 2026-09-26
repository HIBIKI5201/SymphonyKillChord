using Unity.Entities;
using Unity.Mathematics;

namespace KillChord.Runtime.View.InGame.Stage.Barrage
{
    /// <summary>
    ///     弾のワールド空間における速度です。
    /// </summary>
    /// <remarks> 発射時にタレットが設定します。加速や曲進を追加する場合もここを更新します。 </remarks>
    public struct BulletVelocity : IComponentData
    {
        /// <summary> 1秒あたりの移動量です。 </summary>
        public float3 Value;
    }
}
