using Unity.Entities;
using Unity.Transforms;

namespace KillChord.Runtime.View.InGame.Stage.Barrage
{
    /// <summary>
    ///     弾幕演出のシステムをまとめ、ポーズ中は更新を停止するグループです。
    /// </summary>
    /// <remarks> LocalTransformを更新するため、TransformSystemGroupより前に実行します。 </remarks>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public sealed partial class BarrageSystemGroup : ComponentSystemGroup
    {
        /// <summary>
        ///     ポーズ連動のRateManagerを設定します。
        /// </summary>
        protected override void OnCreate()
        {
            base.OnCreate();

            RateManager = new BarragePauseRateManager();
        }
    }
}
