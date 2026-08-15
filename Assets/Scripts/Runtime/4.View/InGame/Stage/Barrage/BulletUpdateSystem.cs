using Unity.Burst;
using Unity.Entities;

namespace KillChord.Runtime.View.InGame.Stage.Barrage
{
    /// <summary>
    ///     弾の移動と寿命処理を並列ジョブでスケジュールします。
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(BarrageSystemGroup))]
    [UpdateAfter(typeof(TurretFireSystem))]
    public partial struct BulletUpdateSystem : ISystem
    {
        /// <summary>
        ///     弾が存在する場合のみ更新するようにします。
        /// </summary>
        /// <param name="state"> システムの状態です。 </param>
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BulletVelocity>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        /// <summary>
        ///     弾の更新ジョブをスケジュールします。
        /// </summary>
        /// <param name="state"> システムの状態です。 </param>
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // 破棄はフレーム終端のECBへ集約し、グループ内に同期ポイントを作らない。
            EntityCommandBuffer commandBuffer =
                SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                    .CreateCommandBuffer(state.WorldUnmanaged);

            BulletUpdateJob job = new()
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                CommandBuffer = commandBuffer.AsParallelWriter(),
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
    }
}
