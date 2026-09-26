using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Stage.Barrage
{
    /// <summary>
    ///     弾のプレハブをEntityへ変換します。
    /// </summary>
    /// <remarks> 速度の向きは発射時にタレットが決めるため、ここでは速さだけを持ちます。 </remarks>
    public sealed class BulletAuthoring : MonoBehaviour
    {
        [SerializeField, Tooltip("弾の初速です。")]
        private float _speed = 10f;

        [SerializeField, Tooltip("弾が消滅するまでの秒数です。")]
        private float _lifetimeSeconds = 3f;

        /// <summary>
        ///     弾のEntity変換を行います。
        /// </summary>
        private sealed class BulletBaker : Baker<BulletAuthoring>
        {
            /// <summary>
            ///     弾の速さ・寿命・速度をEntityへ焼き込みます。
            /// </summary>
            /// <param name="authoring"> 変換元のオーサリングコンポーネントです。 </param>
            public override void Bake(BulletAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new BulletSpeed
                {
                    Value = authoring._speed,
                });

                AddComponent(entity, new BulletLifetime
                {
                    DurationSeconds = authoring._lifetimeSeconds,
                    ElapsedSeconds = 0f,
                });

                AddComponent(entity, new BulletVelocity
                {
                    Value = float3.zero,
                });
            }
        }
    }
}
