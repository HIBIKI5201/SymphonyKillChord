using Unity.Entities;
using Unity.Mathematics;

namespace KillChord.Runtime.View.InGame.Stage.Barrage
{
    /// <summary>
    ///     タレットごとの拡散乱数の状態です。
    /// </summary>
    /// <remarks>
    ///     種はタレットIDから作るため、同じステージでは毎回同じばらけ方になります。
    ///     演出として再現性を優先しています。
    /// </remarks>
    public struct TurretRandom : IComponentData
    {
        /// <summary> 1発ごとに進む乱数の状態です。 </summary>
        public Random Value;
    }
}
