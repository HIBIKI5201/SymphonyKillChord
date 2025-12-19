using Mock.MusicBattle.Battle;
using Mock.MusicBattle.MusicSync;
using Mock.MusicBattle.UI;
using System.Collections.Generic;
using UnityEngine;


namespace Mock.MusicBattle.Enemy
{
    /// <summary>
    ///     エネミー(EnemyManager)生成を一元管理するファクトリ。
    ///     プールを用いてエネミーの生成/再利用を行う。死亡時にプールへ戻す処理もセットアップする。
    /// </summary>
    public class EnemyFactory
    {
        /// <summary>
        ///     <see cref="EnemyFactory"/>クラスの新しいインスタンスを初期化します。
        ///     Factory が利用するコンテナ・ターゲット・プレファブを初期化します。
        ///     Factory 使用前に必ず呼び出す必要がある。
        /// </summary>
        /// <param name="enemyContainer">生成した敵を登録するコンテナ。</param>
        /// <param name="target">敵が追従・攻撃する対象。</param>
        /// <param name="enemyManager">生成元となるエネミーのプレファブ。</param>
        /// <param name="music">音楽同期マネージャー。</param>
        /// <param name="lockOnManager">ロックオンマネージャー。</param>
        /// <param name="hudManager">HUDマネージャー。</param>
        public EnemyFactory(EnemyContainer enemyContainer, Transform target, EnemyManager enemyManager,
            MusicSyncManager music, LockOnManager lockOnManager, IngameHUDManager hudManager)
        {
            _musicManager = music;
            _enemyContainer = enemyContainer;
            _target = target;
            _enemyPrefab = enemyManager;
            _lockOnManager = lockOnManager;
            _hudManager = hudManager;
        }

        // PUBLIC_EVENTS
        // PUBLIC_PROPERTIES
        // INTERFACE_PROPERTIES
        // PUBLIC_CONSTANTS
        #region Publicメソッド
        /// <summary>
        ///     エネミーを生成、またはプールから再利用して返します。
        ///     プールに残っていれば再利用、なければInstantiateします。
        ///     死亡時にプールへ戻すコールバックも設定します。
        /// </summary>
        /// <param name="status">エネミーのステータス。</param>
        /// <param name="position">生成位置。</param>
        /// <returns>生成または再利用されたEnemyManager。</returns>
        public EnemyManager Spawn(EnemyStatus status, Vector3 position)
        {
            if (!_enemyContainer.TryGetFromPool(out var enemy))
            {
                enemy = Object.Instantiate(_enemyPrefab).GetComponent<EnemyManager>();
            }

            enemy.Init(_target, _musicManager, position);
            _ = _hudManager.AddEnemyHealthBar(enemy.HealthEntity, enemy.transform);

            // 死亡イベントハンドラを登録。
            if (_onDeathHandlers.TryGetValue(enemy, out var oldDeathHandler))
            {
                enemy.OnDeath -= oldDeathHandler;
            }
            System.Action deathHandler = () => _lockOnManager.OnTargetLocked -= _onTargetLockedHandlers[enemy];
            _onDeathHandlers[enemy] = deathHandler;
            enemy.OnDeath += deathHandler;

            // OnTargetLockedイベントハンドラを登録。
            if (_onTargetLockedHandlers.TryGetValue(enemy, out var oldTargetHandler))
            {
                _lockOnManager.OnTargetLocked -= oldTargetHandler;
            }
            System.Action<Transform> targetHandler = enemy.SetLockOn;
            _onTargetLockedHandlers[enemy] = targetHandler;
            _lockOnManager.OnTargetLocked += targetHandler;

            _enemyContainer.Register(enemy);

            return enemy;
        }
        #endregion

        // PUBLIC_INTERFACE_METHODS
        // PUBLIC_ENUM_DEFINITIONS
        // PUBLIC_CLASS_DEFINITIONS
        // PUBLIC_STRUCT_DEFINITIONS
        // CONSTANTS
        // INSPECTOR_FIELDS
        #region プライベートフィールド
        /// <summary> HUDマネージャーの参照。 </summary>
        private IngameHUDManager _hudManager;
        /// <summary> エネミープレファブ。 </summary>
        private EnemyManager _enemyPrefab;
        /// <summary> エネミーコンテナ。 </summary>
        private EnemyContainer _enemyContainer;
        /// <summary> 敵が追従・攻撃する対象。 </summary>
        private Transform _target;
        /// <summary> 音楽同期マネージャー。 </summary>
        private MusicSyncManager _musicManager;
        /// <summary> ロックオンマネージャー。 </summary>
        private LockOnManager _lockOnManager;
        /// <summary> 死亡イベントハンドラを格納するDictionary。 </summary>
        private readonly Dictionary<EnemyManager, System.Action> _onDeathHandlers = new();
        /// <summary> ロックオンターゲット変更イベントハンドラを格納するDictionary。 </summary>
        private readonly Dictionary<EnemyManager, System.Action<Transform>> _onTargetLockedHandlers = new();
        #endregion

        // UNITY_LIFECYCLE_METHODS
        // EVENT_HANDLER_METHODS
        // PROTECTED_INTERFACE_VIRTUAL_METHODS
        // PRIVATE_METHODS
        // PRIVATE_ENUM_DEFINITIONS
        // PRIVATE_CLASS_DEFINITIONS
        // PRIVATE_STRUCT_DEFINITIONS
    }
}