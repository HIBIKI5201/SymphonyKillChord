using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     砲弾を生成するインタフェース。
    /// </summary>
    public interface IShellSpawner
    {
        /// <summary>
        ///     砲弾を生成する。
        /// </summary>
        /// <param name="spawnPosition"></param>
        /// <param name="targetPosition"></param>
        public void SpawnShell(EnemyAIController enemyAIController);
    }
}
