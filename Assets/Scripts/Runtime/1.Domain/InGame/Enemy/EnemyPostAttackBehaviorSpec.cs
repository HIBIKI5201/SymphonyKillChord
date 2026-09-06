using System;

namespace KillChord.Runtime.Domain.InGame.Enemy
{
    /// <summary>
    ///     攻撃後の行動選択に関する能力値。
    /// </summary>
    public readonly struct EnemyPostAttackBehaviorSpec
    {
        public EnemyPostAttackBehaviorSpec(
            float stayWeight,
            float regroupWeight,
            float obstacleApproachWeight,
            float regroupDistanceMin,
            float regroupDistanceMax,
            float obstacleApproachRatio,
            float arrivalThreshold)
        {
            if (stayWeight < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(stayWeight), "重みの値は0より小さい。");
            }
            if (regroupWeight < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(regroupWeight), "重みの値は0より小さい。");
            }
            if (obstacleApproachWeight < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(obstacleApproachWeight), "重みの値は0より小さい。");
            }
            if (regroupDistanceMin < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(regroupDistanceMin), "距離の値は0より小さい。");
            }
            if (regroupDistanceMax < regroupDistanceMin)
            {
                throw new ArgumentOutOfRangeException(nameof(regroupDistanceMax), "最大距離が最小距離を下回っている。");
            }
            if (obstacleApproachRatio < 0f || obstacleApproachRatio > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(obstacleApproachRatio), "接近割合の値は0から1の範囲外。");
            }
            if (arrivalThreshold < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(arrivalThreshold), "到達判定距離の値は0より小さい。");
            }

            StayWeight = stayWeight;
            RegroupWeight = regroupWeight;
            ObstacleApproachWeight = obstacleApproachWeight;
            RegroupDistanceMin = regroupDistanceMin;
            RegroupDistanceMax = regroupDistanceMax;
            ObstacleApproachRatio = obstacleApproachRatio;
            ArrivalThreshold = arrivalThreshold;
        }

        /// <summary> その場に留まり再攻撃する重み </summary>
        public float StayWeight { get; }
        /// <summary> 近くの味方に合流する重み </summary>
        public float RegroupWeight { get; }
        /// <summary> 近くの障害物に接近する重み </summary>
        public float ObstacleApproachWeight { get; }
        /// <summary> 合流時、味方から離れる最小距離 </summary>
        public float RegroupDistanceMin { get; }
        /// <summary> 合流時、味方から離れる最大距離 </summary>
        public float RegroupDistanceMax { get; }
        /// <summary> 障害物へ接近する割合(0〜1) </summary>
        public float ObstacleApproachRatio { get; }
        /// <summary> 上書き移動先への到達とみなす距離 </summary>
        public float ArrivalThreshold { get; }
    }
}
