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

        [SerializeField, Tooltip("1発から次の1発までの間隔（秒）です。")]
        private float _fireIntervalSeconds = 0.2f;

        [SerializeField, Tooltip("弾が1発ごとにランダムでばらける円錐の開き角（度）です。0で正面に固定されます。")]
        private float _spreadAngleDegrees = 30f;

        [SerializeField, Tooltip("1回の開始命令で発射する弾数です。0以下の場合は停止命令まで撃ち続けます。")]
        private int _burstCount;

        /// <summary>
        ///     選択中に砲口位置と、弾がばらける円錐の範囲をシーンビューへ描画します。
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Vector3 muzzlePosition = transform.TransformPoint(_muzzleOffsetLocal);

            Gizmos.color = GIZMO_COLOR;
            Gizmos.DrawWireSphere(muzzlePosition, GIZMO_MUZZLE_RADIUS);

            // 拡散0のときに飛ぶ中心の弾道。
            Gizmos.DrawLine(muzzlePosition, GetGizmoPoint(muzzlePosition, 0f, 0f));

            Vector3 firstEdge = Vector3.zero;
            Vector3 previousEdge = Vector3.zero;

            for (int i = 0; i < GIZMO_CONE_SEGMENTS; i++)
            {
                float rollRadians = (2f * Mathf.PI * i) / GIZMO_CONE_SEGMENTS;
                Vector3 edge = GetGizmoPoint(muzzlePosition, 1f, rollRadians);

                // 外周をつないで、ばらける最大範囲を円で示す。
                if (i == 0) { firstEdge = edge; }
                else { Gizmos.DrawLine(previousEdge, edge); }

                // 円錐の形が分かるよう、4方向だけ砲口から稜線を引く。
                if (i % (GIZMO_CONE_SEGMENTS / GIZMO_EDGE_LINE_COUNT) == 0)
                {
                    Gizmos.DrawLine(muzzlePosition, edge);
                }

                previousEdge = edge;
            }

            Gizmos.DrawLine(previousEdge, firstEdge);
        }

        private static readonly Color GIZMO_COLOR = new(1f, 0.5f, 0.2f);

        private const float GIZMO_MUZZLE_RADIUS = 0.1f;

        private const float GIZMO_DIRECTION_LENGTH = 3f;

        private const int GIZMO_CONE_SEGMENTS = 24;

        private const int GIZMO_EDGE_LINE_COUNT = 4;

        /// <summary>
        ///     Gizmo描画用に、拡散円錐上の一点を求めます。
        /// </summary>
        /// <param name="muzzlePosition"> 砲口のワールド座標です。 </param>
        /// <param name="normalizedRadius"> 中心を0、外周を1とした拡散量です。 </param>
        /// <param name="rollRadians"> 円錐断面上のどの向きへ傾けるかを表す角度です。 </param>
        /// <returns> 描画対象となるワールド座標です。 </returns>
        private Vector3 GetGizmoPoint(
            Vector3 muzzlePosition,
            float normalizedRadius,
            float rollRadians)
        {
            // 実行時の発射処理と同じ計算を使い、Gizmoと実際の弾道がずれないようにする。
            float3 direction = BarrageSpread.GetDirection(
                transform.forward,
                transform.up,
                _spreadAngleDegrees,
                normalizedRadius,
                rollRadians);

            return muzzlePosition + (Vector3)(direction * GIZMO_DIRECTION_LENGTH);
        }

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

                int turretId = authoring._turretId.Id;

                AddComponent(entity, new TurretId
                {
                    Value = turretId,
                });

                AddComponent(entity, new TurretConfig
                {
                    BulletPrefab = authoring._bulletPrefab != null
                        ? GetEntity(authoring._bulletPrefab, TransformUsageFlags.Dynamic)
                        : Entity.Null,
                    MuzzleOffsetLocal = authoring._muzzleOffsetLocal,
                    FireIntervalSeconds = authoring._fireIntervalSeconds,
                    SpreadAngleDegrees = authoring._spreadAngleDegrees,
                    BurstCount = authoring._burstCount,
                });

                // タレットごとに違うばらけ方になるよう、IDから乱数の種を作る。
                AddComponent(entity, new TurretRandom
                {
                    Value = Unity.Mathematics.Random.CreateFromIndex((uint)turretId),
                });

                // 発射中だけ有効化する運用のため、無効状態で付与しておく。
                AddComponent<BarrageFireState>(entity);
                SetComponentEnabled<BarrageFireState>(entity, false);
            }
        }
    }
}
