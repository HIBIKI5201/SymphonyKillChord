using Unity.Mathematics;

namespace KillChord.Runtime.View.InGame.Stage.Barrage
{
    /// <summary>
    ///     扇状に広がる弾の発射方向を求めます。
    /// </summary>
    /// <remarks>
    ///     実行時の発射処理とオーサリングのGizmo表示で同じ結果になるよう、計算をここへ集約しています。
    /// </remarks>
    public static class BarrageSpread
    {
        /// <summary>
        ///     指定された同時発射番号に対応する発射方向を求めます。
        /// </summary>
        /// <param name="forward"> タレットの正面方向です。 </param>
        /// <param name="up"> 扇を広げる回転軸です。 </param>
        /// <param name="wayCount"> 同時に撃つ弾数です。 </param>
        /// <param name="spreadAngleDegrees"> 扇全体の開き角（度）です。 </param>
        /// <param name="index"> 0から始まる同時発射番号です。 </param>
        /// <returns> 正規化された発射方向です。 </returns>
        public static float3 GetDirection(
            float3 forward,
            float3 up,
            int wayCount,
            float spreadAngleDegrees,
            int index)
        {
            int count = math.max(wayCount, 1);

            // 1発のみの場合は広げる相手がいないため、正面をそのまま返す。
            if (count <= 1) { return math.normalizesafe(forward, math.forward()); }

            float stepDegrees = spreadAngleDegrees / (count - 1);
            float startDegrees = -spreadAngleDegrees * 0.5f;

            quaternion spread = quaternion.AxisAngle(
                math.normalizesafe(up, math.up()),
                math.radians(startDegrees + (stepDegrees * index)));

            return math.normalizesafe(math.mul(spread, forward), forward);
        }
    }
}
