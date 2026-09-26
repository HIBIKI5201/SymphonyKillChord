using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace KillChord.Runtime.View.InGame.Stage.Barrage
{
    /// <summary>
    ///     弾の移動と寿命処理を1パスでまとめて行います。
    /// </summary>
    /// <remarks>
    ///     移動と寿命でチャンクを2回舐めないよう、同じジョブで処理しています。
    /// </remarks>
    [BurstCompile]
    public partial struct BulletUpdateJob : IJobEntity
    {
        /// <summary> 前回更新からの経過秒数です。 </summary>
        public float DeltaTime;

        /// <summary> 破棄コマンドを積むための並列書き込みバッファです。 </summary>
        public EntityCommandBuffer.ParallelWriter CommandBuffer;

        /// <summary>
        ///     1体分の弾を更新します。
        /// </summary>
        /// <param name="chunkIndex"> 並列書き込みのソートキーになるチャンク番号です。 </param>
        /// <param name="entity"> 更新対象の弾です。 </param>
        /// <param name="transform"> 更新する弾の変換です。 </param>
        /// <param name="lifetime"> 弾の寿命です。 </param>
        /// <param name="velocity"> 弾の速度です。 </param>
        private void Execute(
            [ChunkIndexInQuery] int chunkIndex,
            Entity entity,
            ref LocalTransform transform,
            ref BulletLifetime lifetime,
            in BulletVelocity velocity)
        {
            lifetime.ElapsedSeconds += DeltaTime;

            // 寿命が尽きた弾は移動させずに破棄を予約する。
            if (lifetime.ElapsedSeconds >= lifetime.DurationSeconds)
            {
                CommandBuffer.DestroyEntity(chunkIndex, entity);
                return;
            }

            transform.Position += velocity.Value * DeltaTime;
        }
    }
}
