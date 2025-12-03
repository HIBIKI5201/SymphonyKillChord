using Mock.MusicBattle.Battle;
using Mock.MusicBattle.Enemy;
using Mock.MusicBattle.MusicSync;
using UnityEngine;

namespace Mock.MusicBattle.Basis
{
    /// <summary>Enemyの初期化ユーティリティクラス</summary>
    public static class EnemyInitUtility
    {
        /// <summary>敵の初期化をする。</summary>
        public static EnemyFactory InitEnemy(
            EnemyContainer enemyContainer,
            Transform playerTransform,
            EnemyManager enemyManager,
            MusicSyncManager musicSyncManager,
            LockOnManager lockOnManager
            )
        {
            return new EnemyFactory(
                enemyContainer,
                playerTransform,
                enemyManager,
                musicSyncManager,
                lockOnManager
                );
        }
    }
}
