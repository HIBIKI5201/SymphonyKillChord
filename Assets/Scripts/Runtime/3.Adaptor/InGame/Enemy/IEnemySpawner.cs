using System;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     敵生成インタフェース。
    /// </summary>
    public interface IEnemySpawner
    {
        /// <summary>
        ///     敵生成処理。
        /// </summary>
        public void SpawnEnemy(int amount, Action callback);
    }
}
