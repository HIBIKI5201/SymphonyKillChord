using System;
using System.Collections.Generic;
using KillChord.Runtime.Domain.InGame.Enemy;

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
        /// <param name="enemyDefinitionId"> 生成する個別の敵定義IDです。 </param>
        /// <param name="amount"> 生成数です。 </param>
        /// <param name="candidateSpawnPointHashes"> 候補とするスポーンポイントIDです。null/空の場合は全スポーンポイントが対象です。 </param>
        /// <param name="callback"> 生成完了時に呼ばれます。 </param>
        public void SpawnEnemy(
            EnemyDefinitionId enemyDefinitionId,
            int amount,
            IReadOnlyList<int> candidateSpawnPointHashes,
            Action callback);
    }
}
