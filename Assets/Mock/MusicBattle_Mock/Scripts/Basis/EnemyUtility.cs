using Mock.MusicBattle.Battle;
using Mock.MusicBattle.Camera;
using Mock.MusicBattle.Enemy;
using Mock.MusicBattle.Player;
using System.Collections;
using UnityEngine;

namespace Mock.MusicBattle.Basis
{
    /// <summary>
    ///     敵の全般を管理するユーティリティクラス。
    /// </summary>
    public static class EnemyUtility
    {
        /// <summary>
        ///     敵のスポーンループ処理をする。
        /// </summary>
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

        /// <summary>
        ///     敵コンテナの初期化を行います。
        /// </summary>
        /// <param name="container">初期化するEnemyContainer。</param>
        /// <param name="player">プレイヤーマネージャー。</param>
        /// <param name="cameraManager">カメラマネージャー。</param>
        /// <param name="lockOnManager">ロックオンマネージャー。</param>
        public static void EnemyContainerInit(EnemyContainer container,
            PlayerManager player, CameraManager cameraManager, LockOnManager lockOnManager)
        {
            container.SetCharacter(player);
            container.SetCamera(cameraManager);
            container.SetLockOnManager(lockOnManager);
        }
    }
}
