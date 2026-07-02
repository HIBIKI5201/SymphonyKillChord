using KillChord.Runtime.Adaptor.InGame.Camera.Target;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Application.InGame.Camera.Target;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Composition.InGame.Music;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.View.InGame.Music;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Enemy
{
    /// <summary>
    ///     ボス単体テスト用の初期化ドライバ。
    ///     依存（音楽同期・プレイヤー・ターゲット管理）を集めて
    ///     BossLifeCycle を Initialize → Activate → StartGameplay する。
    ///     ※テスト専用。本番では通常敵と同様にスポナー/ブートストラップへ統合する想定。
    /// </summary>
    public class BossInitializer : MonoBehaviour
    {
        /// <summary>
        ///     初期化処理
        /// </summary>
        /// <param name="targetManager"></param>
        /// <param name="targetEntityRegistry"></param>
        public void Initialize(TargetManager targetManager, TargetEntityRegistry targetEntityRegistry, EnemyPools enemyPools)
        {
            MusicSyncInitializer initializer = FindFirstObjectByType<MusicSyncInitializer>();
            if (initializer == null || initializer.MusicSyncService == null)
            {
                Debug.LogError("MusicSyncInitializerが見つかりません。", this);
                return;
            }

            MusicSyncView musicSyncView = FindAnyObjectByType<MusicSyncView>();
            if (musicSyncView?.MusicSyncState == null)
            {
                Debug.LogError("MusicSyncViewが見つかりません。", this);
                return;
            }
            _musicSyncService = initializer.MusicSyncService;
            _musicSyncState = musicSyncView.MusicSyncState;
            _targetManagerController = new(targetManager);
            _targetEntityRegistryController = new(targetEntityRegistry);
            _playerInitializer = ServiceLocator.GetInstance<PlayerInitializer>();
            if (_playerInitializer == null)
            {
                Debug.LogError("PlayerInitializerの取得に失敗しました。", this);
                return;
            }
            if(enemyPools == null)
            {
                Debug.LogError("EnemyPoolsの取得に失敗しました。", this);
                return;
            }
            _enemyPools = enemyPools;
            // ボス初期化。attackControllerGenerator はボスでは未使用のため null。
            _boss.Initialize(
                _playerInitializer.transform,
                _playerInitializer.PlayerEntity,
                _musicSyncState,
                _musicSyncService,
                _targetManagerController,
                _targetEntityRegistryController,
                null,
                _enemyPools,
                null);
            _initialized = true;
        }

        /// <summary>
        ///     BossLifeCycle取得用のプロパティ。
        /// </summary>
        public BossLifeCycle LifeCycle => _boss;

        [SerializeField]
        private BossLifeCycle _boss;

        private PlayerInitializer _playerInitializer;
        private MusicSyncState _musicSyncState;
        private IMusicSyncService _musicSyncService;
        private TargetManagerController _targetManagerController;
        private TargetEntityRegistryController _targetEntityRegistryController;
        private EnemyPools _enemyPools;
        private bool _initialized = false;
    }
}
