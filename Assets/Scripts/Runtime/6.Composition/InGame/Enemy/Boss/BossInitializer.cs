using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Composition.InGame.Music;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.View.InGame.Music;
using KillChord.Runtime.View.InGame.Target;
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
        public bool Initialize(TargetingSystem targetingSystem, EnemyPools enemyPools)
        {
            if(_boss == null)
            {
                Debug.LogError("BossLifeCycleが見つかりません。", this);
                return false;
            }
            MusicSyncInitializer initializer = FindFirstObjectByType<MusicSyncInitializer>();
            if (initializer == null || initializer.MusicSyncService == null)
            {
                Debug.LogError("MusicSyncInitializerが見つかりません。", this);
                return false;
            }

            MusicSyncView musicSyncView = FindAnyObjectByType<MusicSyncView>();
            if (musicSyncView?.MusicSyncState == null)
            {
                Debug.LogError("MusicSyncViewが見つかりません。", this);
                return false;
            }
            _musicSyncService = initializer.MusicSyncService;
            _musicSyncState = musicSyncView.MusicSyncState;
            _targetingSystem = targetingSystem;
            _playerInitializer = ServiceLocator.GetInstance<PlayerInitializer>();
            if (_playerInitializer == null)
            {
                Debug.LogError("PlayerInitializerの取得に失敗しました。", this);
                return false;
            }
            if(enemyPools == null)
            {
                Debug.LogError("EnemyPoolsの取得に失敗しました。", this);
                return false;
            }
            _enemyPools = enemyPools;
            // ボス初期化。attackControllerGenerator はボスでは未使用のため null。
            _boss.Initialize(
                _playerInitializer.transform,
                _playerInitializer.PlayerEntity,
                _musicSyncState,
                _musicSyncService,
                _targetingSystem,
                null,
                _enemyPools,
                null);
            _initialized = true;
            return true;
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
        private TargetingSystem _targetingSystem;
        private EnemyPools _enemyPools;
        private bool _initialized = false;
    }
}
