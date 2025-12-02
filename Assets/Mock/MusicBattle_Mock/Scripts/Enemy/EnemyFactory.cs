using Mock.MusicBattle.Battle;
using Mock.MusicBattle.MusicSync;
using System.Collections.Generic;
using UnityEngine;


namespace Mock.MusicBattle.Enemy
{
    /// <summary>
    ///     エネミー(EnemyManager)生成を一元管理するファクトリ。
    ///     プールを用いてエネミーの生成/再利用を行う。
    ///     死亡時にプールへ戻す処理もセットアップする。
    /// </summary>
    public class EnemyFactory
    {
        /// <summary>
        ///     Factory が利用するコンテナ・ターゲット・プレファブを初期化する。
        ///     Factory 使用前に必ず呼び出す必要がある。
        /// </summary>
        /// <param name="enemyContainer"> 生成した敵を登録するコンテナ。 </param>
        /// <param name="target"> 敵が追従・攻撃する対象。 </param>
        /// <param name="enemy"> 生成元となるエネミーのプレファブ。 </param>
        public EnemyFactory(EnemyContainer enemyContainer, Transform target, EnemyManager enemyManager,
            MusicSyncManager music, LockOnManager lockonmanager)
        {
            _musicManager = music;
            _enemyContainer = enemyContainer;
            _target = target;
            _enemyPrefab = enemyManager;
            _musicManager = music;
            _lockonmanager = lockonmanager;
        }

        /// <summary>
        ///     エネミーを生成、またはプールから再利用して返す。
        ///     プールに残っていれば再利用
        ///     なければ Instantiate
        ///     死亡時にプールへ戻すコールバックも設定
        /// </summary>
        /// <param name="status"> エネミーのステータス。 </param>
        /// <param name="position"> 生成位置 。</param>
        /// <returns> 生成または再利用された EnemyManager 。</returns>
        public EnemyManager Spawn(EnemyStatus status, Vector3 position)
        {
            if (!_enemyContainer.TryGetFromPool(out var enemy))
            {
                enemy = Object.Instantiate(_enemyPrefab).GetComponent<EnemyManager>();
            }

            enemy.HealthEntity.ResetHealth();
            enemy.SetTarget(_target);
            enemy.InitializeMover();
            enemy.InitMusic(_musicManager);
            enemy.transform.position = position;
            if (_onDeathHandlers.TryGetValue(enemy, out var oldDeathHandler))
            {
                enemy.OnDeath -= oldDeathHandler;
            }

            System.Action deathHandler = () => _lockonmanager.OnTargetLocked -= _onTargetLockedHandlers[enemy];
            _onDeathHandlers[enemy] = deathHandler;
            enemy.OnDeath += deathHandler;

            //  OnTargetLockedイベントの多重登録防止 
            if (_onTargetLockedHandlers.TryGetValue(enemy, out var oldTargetHandler))
            {
                _lockonmanager.OnTargetLocked -= oldTargetHandler;
            }
            System.Action<Transform> targetHandler = enemy.SetLockOn;
            _onTargetLockedHandlers[enemy] = targetHandler;
            _lockonmanager.OnTargetLocked += targetHandler;


            _enemyContainer.Register(enemy);

            return enemy;
        }

        private EnemyManager _enemyPrefab;
        private EnemyContainer _enemyContainer;
        private Transform _target;
        private MusicSyncManager _musicManager;
        private LockOnManager _lockonmanager;
        private Dictionary<EnemyManager, System.Action> _onDeathHandlers = new();
        private Dictionary<EnemyManager, System.Action<Transform>> _onTargetLockedHandlers = new();
    }
}