using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Composition.InGame.Music;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.View.InGame.Character;
using KillChord.Runtime.View.InGame.Enemy;
using KillChord.Runtime.View.InGame.Player;
using SymphonyFrameWork.System.ServiceLocate;
using System.Threading;
using System.Threading.Tasks;
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
        ///     ボス用 Addressables アセットを先行ロードします。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public async Task<bool> ResourceLoadAsync(CancellationToken cancellationToken)
        {
            if (_boss == null)
            {
                Debug.LogError("BossLifeCycleが見つかりません。", this);
                return false;
            }

            return await _boss.LoadAddressableAssetsAsync(cancellationToken);
        }

        /// <summary>
        ///     初期化処理
        /// </summary>
        public bool Initialize(
            TargetSystemController targetingSystem,
            EnemyPools enemyPools,
            DamageNumberPoolView damageNumberPoolView,
            ReusableParticleSystemView damageEffectView)
        {
            if (_boss == null)
            {
                Debug.LogError("BossLifeCycleが見つかりません。", this);
                return false;
            }

            if (damageNumberPoolView == null)
            {
                Debug.LogError("DamageNumberPoolViewが見つかりません。", this);
                return false;
            }

            if (damageEffectView == null)
            {
                Debug.LogError("DamageEffectViewが見つかりません。", this);
                return false;
            }

            MusicSyncModuleContainer musicSyncModuleContainer = ServiceLocator.GetInstance<MusicSyncModuleContainer>();
            if (musicSyncModuleContainer == null || musicSyncModuleContainer.MusicSyncService == null)
            {
                Debug.LogError("MusicSyncInitializerが見つかりません。", this);
                return false;
            }

            if (musicSyncModuleContainer.MusicSyncState == null)
            {
                Debug.LogError("MusicSyncViewが見つかりません。", this);
                return false;
            }
            _musicSyncService = musicSyncModuleContainer.MusicSyncService;
            _musicSyncState = musicSyncModuleContainer.MusicSyncState;
            _targetingSystem = targetingSystem;
            PlayerModuleContainer playerModuleContainer = ServiceLocator.GetInstance<PlayerModuleContainer>();
            _playerInitializer = playerModuleContainer?.PlayerInitializer;
            _playerView = playerModuleContainer?.PlayerView;
            if (_playerInitializer == null || _playerView == null)
            {
                Debug.LogError("PlayerInitializerの取得に失敗しました。", this);
                return false;
            }
            if (enemyPools == null)
            {
                Debug.LogError("EnemyPoolsの取得に失敗しました。", this);
                return false;
            }
            _enemyPools = enemyPools;
            // ボス初期化。attackControllerGenerator はボスでは未使用のため null。
            _boss.Initialize(
                _playerView.transform,
                _playerInitializer.PlayerEntity,
                _musicSyncState,
                _musicSyncService,
                _targetingSystem,
                null,
                _enemyPools,
                damageNumberPoolView,
                damageEffectView,
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
        private PlayerView _playerView;
        private MusicSyncState _musicSyncState;
        private IMusicSyncService _musicSyncService;
        private TargetSystemController _targetingSystem;
        private EnemyPools _enemyPools;
        private bool _initialized = false;
    }
}
