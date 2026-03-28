using KillChord.Runtime.Application;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     敵の移動を制御するコントローラー。
    ///     EnemyMoveUsecaseを呼び出して敵の移動ロジックを実行する。
    /// </summary>
    public class EnemyMoveController
    {
        public EnemyMoveController(EnemyMoveUsecase enemyMoveUsecase)
        {
            _enemyMoveUsecase = enemyMoveUsecase;
        }

        public void Move(Vector3 enemyPosition, Vector3 targetPosition)
        {
            _enemyMoveUsecase.Tick(enemyPosition, targetPosition);
        }

        private readonly EnemyMoveUsecase _enemyMoveUsecase;
    }
}
