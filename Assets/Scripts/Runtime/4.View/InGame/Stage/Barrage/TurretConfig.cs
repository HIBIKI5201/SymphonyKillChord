using Unity.Entities;
using Unity.Mathematics;

namespace KillChord.Runtime.View.InGame.Stage.Barrage
{
    /// <summary>
    ///     タレットの発射設定を保持します。
    /// </summary>
    /// <remarks> Baker時に確定し、実行中は変化しません。 </remarks>
    public struct TurretConfig : IComponentData
    {
        /// <summary> 発射する弾のPrefab Entityです。 </summary>
        public Entity BulletPrefab;

        /// <summary> タレットのローカル空間における砲口位置です。 </summary>
        public float3 MuzzleOffsetLocal;

        /// <summary> 1回の発射から次の発射までの間隔（秒）です。 </summary>
        public float FireIntervalSeconds;

        /// <summary> 1回の発射で同時に撃つ弾数です。 </summary>
        public int WayCount;

        /// <summary> 同時発射する弾を扇状に広げる角度（度）です。 </summary>
        public float SpreadAngleDegrees;

        /// <summary> 1回の開始命令で発射する回数です。0以下の場合は停止命令まで撃ち続けます。 </summary>
        public int BurstCount;
    }
}
