using KillChord.Runtime.Utility.Identity;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Stage.Barrage
{
    /// <summary>
    ///     シーンに設置したタレットをEntityへ変換します。
    /// </summary>
    /// <remarks> SubSceneへ配置して使用します。 </remarks>
    public sealed class TurretAuthoring : MonoBehaviour
    {
        [SerializeField, SourceDataCollection(TurretId.COLLECTION_KEY, true),
         Tooltip("Timelineから発射を命令するときに指定するタレットIDです。")]
        private DataID _turretId;

        [SerializeField, Tooltip("発射する弾のプレハブです。速度と寿命はプレハブ側で設定します。")]
        private GameObject _bulletPrefab;

        [SerializeField, Tooltip("タレットのローカル空間における砲口位置です。")]
        private Vector3 _muzzleOffsetLocal = Vector3.zero;

        [SerializeField, Tooltip("1回の発射から次の発射までの間隔（秒）です。")]
        private float _fireIntervalSeconds = 0.2f;

        [SerializeField, Tooltip("1回の発射で同時に撃つ弾数です。2以上にしないと扇状に広がりません。")]
        private int _wayCount = 1;

        [SerializeField, Tooltip("同時発射する弾を扇状に広げる角度（度）です。同時発射数が1の場合は効果がありません。")]
        private float _spreadAngleDegrees = 30f;

        [SerializeField, Tooltip("1回の開始命令で発射する回数です。0以下の場合は停止命令まで撃ち続けます。")]
        private int _burstCount;

        /// <summary>
        ///     選択中に砲口位置と、同時発射する弾の弾道をシーンビューへ描画します。
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Vector3 muzzlePosition = transform.TransformPoint(_muzzleOffsetLocal);

            Gizmos.color = GIZMO_COLOR;
            Gizmos.DrawWireSphere(muzzlePosition, GIZMO_MUZZLE_RADIUS);

            int wayCount = Mathf.Max(_wayCount, 1);
            Vector3 previousEnd = Vector3.zero;

            for (int i = 0; i < wayCount; i++)
            {
                // 実行時の発射処理と同じ計算を使い、Gizmoと実際の弾道がずれないようにする。
                float3 direction = BarrageSpread.GetDirection(
                    transform.forward,
                    transform.up,
                    wayCount,
                    _spreadAngleDegrees,
                    i);

                Vector3 end = muzzlePosition + (Vector3)(direction * GIZMO_DIRECTION_LENGTH);
                Gizmos.DrawLine(muzzlePosition, end);

                // 扇の開き具合が分かるよう、隣り合う弾道の先端をつなぐ。
                if (i > 0) { Gizmos.DrawLine(previousEnd, end); }

                previousEnd = end;
            }
        }

        private static readonly Color GIZMO_COLOR = new(1f, 0.5f, 0.2f);

        private const float GIZMO_MUZZLE_RADIUS = 0.1f;

        private const float GIZMO_DIRECTION_LENGTH = 3f;

        /// <summary>
        ///     タレットのEntity変換を行います。
        /// </summary>
        private sealed class TurretBaker : Baker<TurretAuthoring>
        {
            /// <summary>
            ///     タレットの識別子と発射設定をEntityへ焼き込みます。
            /// </summary>
            /// <param name="authoring"> 変換元のオーサリングコンポーネントです。 </param>
            public override void Bake(TurretAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new TurretId
                {
                    Value = authoring._turretId.Id,
                });

                AddComponent(entity, new TurretConfig
                {
                    BulletPrefab = authoring._bulletPrefab != null
                        ? GetEntity(authoring._bulletPrefab, TransformUsageFlags.Dynamic)
                        : Entity.Null,
                    MuzzleOffsetLocal = authoring._muzzleOffsetLocal,
                    FireIntervalSeconds = authoring._fireIntervalSeconds,
                    WayCount = authoring._wayCount,
                    SpreadAngleDegrees = authoring._spreadAngleDegrees,
                    BurstCount = authoring._burstCount,
                });

                // 発射中だけ有効化する運用のため、無効状態で付与しておく。
                AddComponent<BarrageFireState>(entity);
                SetComponentEnabled<BarrageFireState>(entity, false);
            }
        }
    }
}
