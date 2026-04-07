using KillChord.Runtime.Domain.InGame.Enemy;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Enemy
{
    /// <summary>
    ///     敵の追従移動ロジック。
    /// </summary>
    public class EnemyMoveUsecase
    {
        public EnemyMoveUsecase(EnemyMoveSpec enemyMoveSpec)
        {
            _enemyMoveSpec = enemyMoveSpec;
        }

        public EnemyMoveDecision Evaluate(Vector3 enemyPosition, Vector3 playerPosition)
        {
            float distance = Vector3.Distance(enemyPosition, playerPosition);

            if (distance > _enemyMoveSpec.AttackRange.Value)
            {
                return new EnemyMoveDecision(
                    true,
                    playerPosition,
                    _enemyMoveSpec.MoveSpeed.Value
                );
            }

            return new EnemyMoveDecision(
                false,
                enemyPosition,
                0f
            );
        }

        private readonly EnemyMoveSpec _enemyMoveSpec;
    }
}
