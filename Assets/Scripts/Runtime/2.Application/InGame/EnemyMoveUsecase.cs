using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    /// <summary>
    ///     敵の追従移動ロジック。
    /// </summary>
    public class EnemyMoveUsecase
    {
        public EnemyMoveUsecase(EnemyMoveSpec enemyMoveSpec, IEnemyNavigationAgent enemyNavigationAgent)
        {
            _enemyMoveSpec = enemyMoveSpec;
            _enemyNavigationAgent = enemyNavigationAgent;
        }

        public void Tick(Vector3 enemyPosition, Vector3 playerPosition)
        {
            if (!_enemyNavigationAgent.IsReady)
            {
                Debug.Log("EnemyNavigationAgent is not ready.");
                return;
            }

            float distanceToPlayer = Vector3.Distance(enemyPosition, playerPosition);
            Debug.Log($"Distance to player: {distanceToPlayer}");

            if (distanceToPlayer <= _enemyMoveSpec.AttackRange.Value)
            {
                Debug.Log("攻撃範囲に入った。");
                _enemyNavigationAgent.Stop();
                return;
            }

            Debug.Log("プレイヤーを追従中...");
            _enemyNavigationAgent.SetMoveSpeed(_enemyMoveSpec.MoveSpeed.Value);
            _enemyNavigationAgent.MoveTo(playerPosition);
        }

        private readonly EnemyMoveSpec _enemyMoveSpec;  
        private readonly IEnemyNavigationAgent _enemyNavigationAgent;
    }
}
