using KillChord.Runtime.Domain.InGame.Enemy;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Enemy
{
    /// <summary>
    ///     攻撃後の行動(再攻撃/合流/障害物接近)を決定するロジック。
    /// </summary>
    public class EnemyPostAttackBehaviorUsecase
    {
        public EnemyPostAttackBehaviorUsecase(EnemyPostAttackBehaviorSpec spec)
        {
            _spec = spec;
        }

        /// <summary> 上書き移動先への到達とみなす距離 </summary>
        public float ArrivalThreshold => _spec.ArrivalThreshold;

        /// <summary>
        ///     攻撃後の行動を抽選し、移動が必要な場合は移動先を返す。
        /// </summary>
        /// <param name="selfPosition"></param>
        /// <param name="nearestAllyPosition"></param>
        /// <param name="nearestObstaclePosition"></param>
        /// <param name="overrideDestination"></param>
        /// <returns> 移動が必要な場合はtrue。 </returns>
        public bool TryDecideOverrideDestination(
            Vector3 selfPosition,
            Vector3? nearestAllyPosition,
            Vector3? nearestObstaclePosition,
            out Vector3 overrideDestination)
        {
            EnemyPostAttackBehaviorKind kind = ChooseKind(nearestAllyPosition.HasValue, nearestObstaclePosition.HasValue);

            if (kind == EnemyPostAttackBehaviorKind.RegroupWithAlly)
            {
                overrideDestination = CalculateRegroupDestination(selfPosition, nearestAllyPosition.Value);
                return true;
            }

            if (kind == EnemyPostAttackBehaviorKind.ApproachObstacle)
            {
                overrideDestination = CalculateObstacleApproachDestination(selfPosition, nearestObstaclePosition.Value);
                return true;
            }

            overrideDestination = selfPosition;
            return false;
        }

        /// <summary>
        ///     重みに基づき行動の種類を抽選する。
        /// </summary>
        private EnemyPostAttackBehaviorKind ChooseKind(bool hasAlly, bool hasObstacle)
        {
            float stayWeight = _spec.StayWeight;
            float regroupWeight = hasAlly ? _spec.RegroupWeight : 0f;
            float obstacleWeight = hasObstacle ? _spec.ObstacleApproachWeight : 0f;
            float totalWeight = stayWeight + regroupWeight + obstacleWeight;

            // 抽選可能な重みが無い場合はその場に留まる。
            if (totalWeight <= 0f)
            {
                return EnemyPostAttackBehaviorKind.Stay;
            }

            float roll = Random.Range(0f, totalWeight);
            if (roll < stayWeight)
            {
                return EnemyPostAttackBehaviorKind.Stay;
            }
            if (roll < stayWeight + regroupWeight)
            {
                return EnemyPostAttackBehaviorKind.RegroupWithAlly;
            }
            return EnemyPostAttackBehaviorKind.ApproachObstacle;
        }

        /// <summary>
        ///     味方から一定距離だけ離れた合流地点を計算する。
        /// </summary>
        private Vector3 CalculateRegroupDestination(Vector3 selfPosition, Vector3 allyPosition)
        {
            Vector3 direction = selfPosition - allyPosition;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                // 味方と完全に重なっている場合のフォールバック方向。
                direction = Vector3.forward;
            }
            direction.Normalize();

            float distance = Random.Range(_spec.RegroupDistanceMin, _spec.RegroupDistanceMax);
            return allyPosition + direction * distance;
        }

        /// <summary>
        ///     障害物に一定割合だけ近づいた地点を計算する。
        /// </summary>
        private Vector3 CalculateObstacleApproachDestination(Vector3 selfPosition, Vector3 obstaclePosition)
        {
            return Vector3.Lerp(selfPosition, obstaclePosition, _spec.ObstacleApproachRatio);
        }

        private readonly EnemyPostAttackBehaviorSpec _spec;
    }
}
