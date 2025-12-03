using Mock.MusicBattle.Enemy;
using System.Collections;
using UnityEngine;

namespace Mock.MusicBattle.Basis
{
    /// <summary>Enemyの全般を管理するユーティリティクラス</summary>
    public static class EnemyUtility
    {
        /// <summary>Enemyのスポーンループ処理をする。</summary>
        public static IEnumerator SpawnLoop(
            EnemyContainer enemyContainer,
            EnemySpawnSO spawnSO,
            EnemyFactory factory,
            EnemyStatus status,
            float enemySpawnTime
            )
        {
            while (true)
            {
                if (enemyContainer.Targets.Count < spawnSO.MaxEnemyCount)
                {
                    Vector3 randomPos = new Vector3(
                        Random.Range(-spawnSO.XRange, spawnSO.XRange),
                        spawnSO.YRange,
                        Random.Range(-spawnSO.ZRange, spawnSO.ZRange));

                    factory.Spawn(status, randomPos);
                }

                yield return new WaitForSeconds(enemySpawnTime);
            }
        }
    }
}
