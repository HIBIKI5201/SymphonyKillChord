using Mock.MusicBattle.Enemy;
using UnityEngine;

namespace Mock.MusicBattle.Basis
{
    /// <summary>Enemyのスポーン処理を行うユーティリティクラス</summary>
    public static class EnemySpawnUtility
    {
        /// <summary>スポーン位置を生成する</summary>
        public static Vector3 CreateSpawnPos(EnemySpawnSO spawnSO)
        {
            return new Vector3(
                Random.Range(-spawnSO.XRange, spawnSO.XRange),
                        -spawnSO.YRange,
                        Random.Range(-spawnSO.ZRange, spawnSO.ZRange)
            );
        }

        /// <summary>Enemyをスポーンする。</summary>
        public static void SpawnEnemy(
            EnemyFactory factory,
            EnemyStatus status,
            Vector3 pos
            )
        {
            factory.Spawn(status, pos);
        }
    }
}
