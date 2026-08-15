using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace KillChord.Runtime.View.InGame.Stage.Barrage
{
    /// <summary>
    ///     Timelineから発行された弾幕コマンドを、IDで対応するタレットへ振り分けます。
    /// </summary>
    /// <remarks>
    ///     SubSceneのタレットはGameObject参照で指定できないため、数値IDと辞書で解決します。
    /// </remarks>
    [BurstCompile]
    [UpdateInGroup(typeof(BarrageSystemGroup))]
    public partial struct TurretRequestRoutingSystem : ISystem
    {
        /// <summary>
        ///     ID解決用の辞書を確保します。
        /// </summary>
        /// <param name="state"> システムの状態です。 </param>
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            // 容量は再確保を減らすための初期値であり、上限ではありません。
            _turretMap = new NativeHashMap<int, Entity>(INITIAL_TURRET_CAPACITY, Allocator.Persistent);

            _unregisteredTurretQuery = SystemAPI.QueryBuilder()
                .WithAll<TurretId>()
                .WithNone<TurretRegistered>()
                .Build();
            _destroyedTurretQuery = SystemAPI.QueryBuilder()
                .WithAll<TurretRegistered>()
                .WithNone<TurretId>()
                .Build();
            _commandQuery = SystemAPI.QueryBuilder()
                .WithAll<BarrageFireCommand>()
                .Build();
        }

        /// <summary>
        ///     ID解決用の辞書を解放します。
        /// </summary>
        /// <param name="state"> システムの状態です。 </param>
        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            if (_turretMap.IsCreated) { _turretMap.Dispose(); }
        }

        /// <summary>
        ///     タレットの登録状況を更新し、コマンドを振り分けます。
        /// </summary>
        /// <param name="state"> システムの状態です。 </param>
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // 登録も破棄もコマンドも無いフレームでは、ECBの確保すら行わない。
            if (_unregisteredTurretQuery.IsEmpty
                && _destroyedTurretQuery.IsEmpty
                && _commandQuery.IsEmpty)
            {
                return;
            }

            EntityCommandBuffer commandBuffer = new(state.WorldUpdateAllocator);

            // 破棄済みの登録を先に落としてから、当フレームのコマンドを解決する。
            UnregisterDestroyedTurrets(ref state, ref commandBuffer);
            RegisterNewTurrets(ref state, ref commandBuffer);
            RouteCommands(ref state, ref commandBuffer);

            commandBuffer.Playback(state.EntityManager);
        }

        private const int INITIAL_TURRET_CAPACITY = 32;

        private NativeHashMap<int, Entity> _turretMap;

        private EntityQuery _unregisteredTurretQuery;

        private EntityQuery _destroyedTurretQuery;

        private EntityQuery _commandQuery;

        /// <summary>
        ///     破棄されたタレットの登録を辞書から除去します。
        /// </summary>
        /// <param name="state"> システムの状態です。 </param>
        /// <param name="commandBuffer"> 構造変化を積むコマンドバッファです。 </param>
        private void UnregisterDestroyedTurrets(
            ref SystemState state,
            ref EntityCommandBuffer commandBuffer)
        {
            // Entity破棄後はCleanupコンポーネントだけが残るため、TurretIdの有無で検出できる。
            foreach ((RefRO<TurretRegistered> registered, Entity entity)
                     in SystemAPI.Query<RefRO<TurretRegistered>>()
                         .WithNone<TurretId>()
                         .WithEntityAccess())
            {
                _turretMap.Remove(registered.ValueRO.Id);
                commandBuffer.RemoveComponent<TurretRegistered>(entity);
            }
        }

        /// <summary>
        ///     未登録のタレットを辞書へ登録します。
        /// </summary>
        /// <param name="state"> システムの状態です。 </param>
        /// <param name="commandBuffer"> 構造変化を積むコマンドバッファです。 </param>
        private void RegisterNewTurrets(
            ref SystemState state,
            ref EntityCommandBuffer commandBuffer)
        {
            foreach ((RefRO<TurretId> turretId, Entity entity)
                     in SystemAPI.Query<RefRO<TurretId>>()
                         .WithNone<TurretRegistered>()
                         .WithEntityAccess())
            {
                int id = turretId.ValueRO.Value;

                // 同フレームのコマンドから解決できるよう、辞書へは即時に反映する。
                _turretMap[id] = entity;
                commandBuffer.AddComponent(entity, new TurretRegistered { Id = id });
            }
        }

        /// <summary>
        ///     弾幕コマンドを対象のタレットへ適用します。
        /// </summary>
        /// <param name="state"> システムの状態です。 </param>
        /// <param name="commandBuffer"> 構造変化を積むコマンドバッファです。 </param>
        private void RouteCommands(
            ref SystemState state,
            ref EntityCommandBuffer commandBuffer)
        {
            foreach ((RefRO<BarrageFireCommand> command, Entity commandEntity)
                     in SystemAPI.Query<RefRO<BarrageFireCommand>>().WithEntityAccess())
            {
                // 宛先が見つからない場合もコマンドは滞留させず、1フレームで消費する。
                commandBuffer.DestroyEntity(commandEntity);

                if (!_turretMap.TryGetValue(command.ValueRO.TargetTurretId, out Entity turret)) { continue; }
                if (!state.EntityManager.Exists(turret)) { continue; }

                if (command.ValueRO.Kind == BarrageCommandKind.Stop)
                {
                    commandBuffer.SetComponentEnabled<BarrageFireState>(turret, false);
                    continue;
                }

                TurretConfig config = SystemAPI.GetComponent<TurretConfig>(turret);
                commandBuffer.SetComponent(turret, new BarrageFireState
                {
                    RemainingShots = config.BurstCount > 0
                        ? config.BurstCount
                        : BarrageFireState.INFINITE_SHOTS,

                    // 命令が届いたフレームで1発目を撃つ。
                    Timer = 0f,
                });
                commandBuffer.SetComponentEnabled<BarrageFireState>(turret, true);
            }
        }
    }
}
