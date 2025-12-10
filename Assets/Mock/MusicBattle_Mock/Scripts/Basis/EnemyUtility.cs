using Mock.MusicBattle.Battle;
using Mock.MusicBattle.Camera;
using Mock.MusicBattle.Enemy;
using Mock.MusicBattle.Player;
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
                        Random.Range(-spawnSO.RangeX, spawnSO.RangeX),
                        spawnSO.RangeY,
                        Random.Range(-spawnSO.RangeZ, spawnSO.RangeZ));

                    factory.Spawn(status, randomPos);
                }

                yield return new WaitForSeconds(enemySpawnTime);
            }
        }

        public static void EnemyContainerInit(EnemyContainer container
            ,PlayerManager player,CameraManager camera,LockOnManager lockon)
        {
            container.SetCharacter(player);
            container.SetCamera(camera);
            container.SetLockOnManager(lockon);
        }
    }
}
