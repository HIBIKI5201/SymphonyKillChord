using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    public interface IEnemySpawner
    {
        /// <summary>
        ///     敵生成処理。
        /// </summary>
        public void SpawnEnemy(int amount);
    }
}
