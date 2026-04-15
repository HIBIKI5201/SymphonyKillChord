using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Enemy;
using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Enemy
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
        public EnemyMoveUsecase(EnemyMoveSpec enemyMoveSpec)
        {
            _enemyMoveSpec = enemyMoveSpec;
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

            // 攻撃範囲外であれば、プレイヤーに向かって移動する。
            if (distance > _enemyMoveSpec.AttackRange.Value)
            {
                return new EnemyMoveDecision(
                    true,
                    playerPosition,
                    _enemyMoveSpec.MoveSpeed.Value
                );
            }

            // 攻撃範囲内であれば、移動しない。
            return new EnemyMoveDecision(
                false,
                enemyPosition,
                0f
            );
        }

        private readonly EnemyMoveSpec _enemyMoveSpec;
    }
}
