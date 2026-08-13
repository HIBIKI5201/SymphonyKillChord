using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace KillChord.Runtime.View.InGame.Stage.Barrage
{
    /// <summary>
    ///     発射中のタレットから、設定された間隔で弾を1発ずつ生成します。
    /// </summary>
    /// <remarks>
    ///     タレット数はステージ内で数十程度を想定しているため、
    ///     ジョブ化の待ち合わせコストが上回らないようメインスレッドで処理します。
    /// </remarks>
    [BurstCompile]
    [UpdateInGroup(typeof(BarrageSystemGroup))]
    [UpdateAfter(typeof(TurretRequestRoutingSystem))]
    public partial struct TurretFireSystem : ISystem
    {
        /// <summary>
        ///     発射中のタレットが存在する場合のみ更新するようにします。
        /// </summary>
        /// <param name="state"> システムの状態です。 </param>
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BarrageFireState>();
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        /// <summary>
        ///     発射タイマーを進め、期限が来たタレットから弾を生成します。
        /// </summary>
        /// <param name="state"> システムの状態です。 </param>
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            // 生成は次フレーム冒頭のECBへ集約する。
            // TransformSystemGroupより前に再生されるため、初回描画からLocalToWorldが正しく求まる。
            EntityCommandBuffer commandBuffer =
                SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                    .CreateCommandBuffer(state.WorldUnmanaged);

            foreach ((RefRW<BarrageFireState> fireState, RefRW<TurretRandom> turretRandom, RefRO<TurretConfig> config, RefRO<LocalToWorld> localToWorld, Entity entity)
                     in SystemAPI.Query<RefRW<BarrageFireState>, RefRW<TurretRandom>, RefRO<TurretConfig>, RefRO<LocalToWorld>>()
                         .WithEntityAccess())
            {
                TurretConfig turretConfig = config.ValueRO;

                // 間隔が0以下だと発射ループが終わらないため下限を設ける。
                float interval = math.max(turretConfig.FireIntervalSeconds, MINIMUM_FIRE_INTERVAL_SECONDS);

                fireState.ValueRW.Timer -= deltaTime;

                // 弾の性能はプレハブ側に持たせているため、発射ごとではなくタレット単位で一度だけ読む。
                bool canFire = turretConfig.BulletPrefab != Entity.Null
                    && SystemAPI.HasComponent<BulletSpeed>(turretConfig.BulletPrefab);
                LocalTransform prefabTransform = default;
                float bulletSpeed = 0f;
                if (canFire)
                {
                    prefabTransform = SystemAPI.GetComponent<LocalTransform>(turretConfig.BulletPrefab);
                    bulletSpeed = SystemAPI.GetComponent<BulletSpeed>(turretConfig.BulletPrefab).Value;
                }

                Random random = turretRandom.ValueRO.Value;

                // 1フレームに複数回の発射タイミングが重なっても取りこぼさない。
                while (fireState.ValueRO.Timer <= 0f && fireState.ValueRO.RemainingShots != 0)
                {
                    if (canFire)
                    {
                        FireBullet(
                            ref commandBuffer,
                            ref random,
                            turretConfig,
                            localToWorld.ValueRO,
                            prefabTransform,
                            bulletSpeed);
                    }

                    fireState.ValueRW.Timer += interval;

                    if (fireState.ValueRO.RemainingShots > 0)
                    {
                        fireState.ValueRW.RemainingShots--;
                    }
                }

                // 進めた乱数の状態を書き戻し、次の発射で同じ方向へ飛ばないようにする。
                turretRandom.ValueRW.Value = random;

                // 撃ち切ったタレットは次フレームからクエリ対象外にする。
                if (fireState.ValueRO.RemainingShots == 0)
                {
                    commandBuffer.SetComponentEnabled<BarrageFireState>(entity, false);
                }
            }
        }

        private const float MINIMUM_FIRE_INTERVAL_SECONDS = 0.01f;

        /// <summary>
        ///     拡散円錐内のランダムな方向へ弾を1発生成します。
        /// </summary>
        /// <param name="commandBuffer"> 生成コマンドを積むコマンドバッファです。 </param>
        /// <param name="random"> 発射ごとに進める乱数の状態です。 </param>
        /// <param name="config"> タレットの発射設定です。 </param>
        /// <param name="localToWorld"> タレットの現在のワールド変換です。 </param>
        /// <param name="prefabTransform"> 弾プレハブの変換です。スケールの引き継ぎに使用します。 </param>
        /// <param name="bulletSpeed"> 弾プレハブに設定された初速です。 </param>
        private static void FireBullet(
            ref EntityCommandBuffer commandBuffer,
            ref Random random,
            in TurretConfig config,
            in LocalToWorld localToWorld,
            in LocalTransform prefabTransform,
            float bulletSpeed)
        {
            // 砲口はローカル指定なので、その時点のワールド変換で解決する。
            float3 muzzlePosition = math.transform(localToWorld.Value, config.MuzzleOffsetLocal);
            float3 up = localToWorld.Up;

            float3 direction = BarrageSpread.GetRandomDirection(
                ref random,
                localToWorld.Forward,
                up,
                config.SpreadAngleDegrees);

            Entity bullet = commandBuffer.Instantiate(config.BulletPrefab);

            // プレハブのスケールを維持したまま、位置と向きだけ差し替える。
            LocalTransform bulletTransform = prefabTransform;
            bulletTransform.Position = muzzlePosition;
            bulletTransform.Rotation = quaternion.LookRotationSafe(direction, up);

            commandBuffer.SetComponent(bullet, bulletTransform);
            commandBuffer.SetComponent(bullet, new BulletVelocity
            {
                Value = direction * bulletSpeed,
            });
        }
    }
}
