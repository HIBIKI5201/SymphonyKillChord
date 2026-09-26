using Unity.Entities;

namespace KillChord.Runtime.View.InGame.Stage.Barrage
{
    /// <summary>
    ///     弾の初速です。
    /// </summary>
    /// <remarks> 弾の性能なのでプレハブ側に持たせ、発射時にタレットが読み取ります。 </remarks>
    public struct BulletSpeed : IComponentData
    {
        /// <summary> 発射時に与える速さです。 </summary>
        public float Value;
    }
}
