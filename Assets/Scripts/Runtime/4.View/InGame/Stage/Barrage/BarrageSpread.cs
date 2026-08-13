using Unity.Mathematics;

namespace KillChord.Runtime.View.InGame.Stage.Barrage
{
    /// <summary>
    ///     正面を軸とした円錐内に弾を拡散させる方向を求めます。
    /// </summary>
    /// <remarks>
    ///     実行時の発射処理とオーサリングのGizmo表示で同じ結果になるよう、計算をここへ集約しています。
    /// </remarks>
    public static class BarrageSpread
    {
        /// <summary>
        ///     拡散円錐内の指定位置に対応する発射方向を求めます。
        /// </summary>
        /// <param name="forward"> タレットの正面方向です。 </param>
        /// <param name="up"> 円錐の基準となる上方向です。 </param>
        /// <param name="spreadAngleDegrees"> 円錐全体の開き角（度）です。 </param>
        /// <param name="normalizedRadius"> 中心を0、外周を1とした拡散量です。 </param>
        /// <param name="rollRadians"> 円錐断面上のどの向きへ傾けるかを表す角度です。 </param>
        /// <returns> 正規化された発射方向です。 </returns>
        public static float3 GetDirection(
            float3 forward,
            float3 up,
            float spreadAngleDegrees,
            float normalizedRadius,
            float rollRadians)
        {
            float3 baseForward = math.normalizesafe(forward, math.forward());
            float angle = math.radians(spreadAngleDegrees) * 0.5f * math.saturate(normalizedRadius);

            if (angle <= 0f) { return baseForward; }

            // 正面に直交する基底を作り、その断面上のどの向きへ傾けるかをrollで決める。
            float3 right = math.normalizesafe(math.cross(up, baseForward));

            // 正面と上方向が平行な場合は基底を作れないため、別の軸で代用する。
            if (math.lengthsq(right) <= 0f)
            {
                right = math.normalizesafe(math.cross(math.right(), baseForward), math.up());
            }

            float3 planeUp = math.cross(baseForward, right);
            float3 offsetDirection = (math.cos(rollRadians) * right) + (math.sin(rollRadians) * planeUp);

            return math.normalizesafe(
                (baseForward * math.cos(angle)) + (offsetDirection * math.sin(angle)),
                baseForward);
        }

        /// <summary>
        ///     拡散円錐内からランダムな発射方向を1つ求めます。
        /// </summary>
        /// <param name="random"> 発射ごとに進める乱数の状態です。 </param>
        /// <param name="forward"> タレットの正面方向です。 </param>
        /// <param name="up"> 円錐の基準となる上方向です。 </param>
        /// <param name="spreadAngleDegrees"> 円錐全体の開き角（度）です。 </param>
        /// <returns> 正規化された発射方向です。 </returns>
        public static float3 GetRandomDirection(
            ref Random random,
            float3 forward,
            float3 up,
            float spreadAngleDegrees)
        {
            // 半径をそのまま乱数にすると中心へ寄るため、平方根を取って断面内で偏りなくばらけさせる。
            float normalizedRadius = math.sqrt(random.NextFloat());
            float rollRadians = random.NextFloat(0f, 2f * math.PI);

            return GetDirection(forward, up, spreadAngleDegrees, normalizedRadius, rollRadians);
        }
    }
}
