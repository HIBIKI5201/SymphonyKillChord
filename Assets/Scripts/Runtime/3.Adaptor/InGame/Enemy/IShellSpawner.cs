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
        /// <param name="enemyAIController"></param>
        public void SpawnShell(EnemyAIController enemyAIController);
    }
}
