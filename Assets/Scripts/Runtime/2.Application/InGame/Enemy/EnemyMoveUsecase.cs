using KillChord.Runtime.Domain.InGame.Enemy;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Enemy
{
    /// <summary>
    ///     敵の追従移動ロジック。
    /// </summary>
    public class EnemyMoveUsecase
    {
        /// <summary>
        ///     敵の移動に関する仕様を受け取るコンストラクタ。
        /// </summary>
        /// <param name="enemyMoveSpec"></param>
        public EnemyMoveUsecase(EnemyMoveSpec enemyMoveSpec, EnemyRaycastDetectService raycastDetector, NearestAttackPositionSearchService nearestAttackPositionSearcher)
        {
            _enemyMoveSpec = enemyMoveSpec;
            _raycastDetector = raycastDetector;
            _nearestAttackPositionSearcher = nearestAttackPositionSearcher;
        }

        /// <summary>
        ///     移動の意思決定を行うメソッド。
        ///     敵とプレイヤーの位置を受け取り、移動すべきか、目的地、速度を返す。
        /// </summary>
        /// <param name="enemyPosition"></param>
        /// <param name="playerPosition"></param>
        /// <returns></returns>
        public EnemyMoveDecision Evaluate(Vector3 enemyPosition, Vector3 playerPosition)
        {
            float distance = Vector3.Distance(enemyPosition, playerPosition);

            // 攻撃範囲外の場合、プレイヤーに向かって移動する。
            if (distance > _enemyMoveSpec.AttackRangeMax.Value)
            {
                return new EnemyMoveDecision(
                    true,
                    playerPosition,
                    _enemyMoveSpec.MoveSpeed.Value
                );
            }
            // 攻撃範囲内、かつプレイヤーに近すぎる場合、最も近い攻撃位置を探索して移動する
            if (distance < _enemyMoveSpec.AttackRangeMin.Value)
            {
                Vector3 nearestAttackPosition = _nearestAttackPositionSearcher.FindNearestAttackPosition(
                    enemyPosition,
                    playerPosition,
                    _enemyMoveSpec.AttackRangeMin.Value);
                return new EnemyMoveDecision(
                    true,
                    nearestAttackPosition,
                    _enemyMoveSpec.MoveSpeed.Value
                );
            }
            // 攻撃範囲内、かつプレイヤーとの間に障害物がある場合、迂回して移動し続ける
            if(!_raycastDetector.CanRaycastHitTarget)
            {
                return new EnemyMoveDecision(
                    true,
                    playerPosition,
                    _enemyMoveSpec.MoveSpeed.Value
                );
            }

            // 攻撃範囲内、かつプレイヤーとの間に障害物がない場合、移動しない。
            return new EnemyMoveDecision(
                false,
                enemyPosition,
                0f
            );
        }

        /// <summary>
        ///     プレイヤーが敵の攻撃範囲内か判定する。
        /// </summary>
        /// <param name="enemyPosition"></param>
        /// <param name="playerPosition"></param>
        /// <returns></returns>
        public bool IsPlayerInAttackRange(Vector3 enemyPosition, Vector3 playerPosition)
        {
            float distance = Vector3.Distance(enemyPosition, playerPosition);
            return distance <= _enemyMoveSpec.AttackRangeMax.Value
                && distance >= _enemyMoveSpec.AttackRangeMin.Value;
        }

        private readonly EnemyMoveSpec _enemyMoveSpec;
        private readonly EnemyRaycastDetectService _raycastDetector;
        private readonly NearestAttackPositionSearchService _nearestAttackPositionSearcher;
    }
}
