//using KillChord.Runtime.Adaptor.InGame.Target;
//using KillChord.Runtime.Adaptor.InGame.Music;
//using KillChord.Runtime.Application.InGame.Music;
//using KillChord.Runtime.Composition.InGame.Enemy;
//using KillChord.Runtime.Composition.InGame.Music;
//using KillChord.Runtime.Composition.InGame.Player;
//using KillChord.Runtime.View.InGame.Music;
//using KillChord.Runtime.View.InGame.Target;
//using SymphonyFrameWork.System.ServiceLocate;
//using UnityEngine;

//namespace DevelopProducts.Boss
//{
//    /// <summary>
//    ///     ボス単体テスト用の初期化ドライバ。
//    ///     依存（音楽同期・プレイヤー・ターゲット管理）を集めて
//    ///     BossLifeCycle を Initialize → Activate → StartGameplay する。
//    ///     ※テスト専用。本番では通常敵と同様にスポナー/ブートストラップへ統合する想定。
//    /// </summary>
//    [DefaultExecutionOrder(100)] // IngameComposition(-100) より後に走らせる
//    public class BossTestInitializer : MonoBehaviour
//    {
//        [SerializeField, Tooltip("初期化するボス")] private BossLifeCycle _boss;
//        [SerializeField, Tooltip("ボスの出現位置（NavMesh上）")] private Transform _spawnPoint;
//        [SerializeField, Tooltip("砲撃攻撃を使う場合に必要。なければ空でOK")] private EnemyPools _enemyPools;

//        private async void Start()
//        {
//            // プレイヤー・音楽同期が立ち上がるまで待つ。
//            PlayerInitializer playerInitializer = await WaitServiceAsync<PlayerInitializer>();
//            if (playerInitializer == null)
//            {
//                Debug.LogError("[BossTestInitializer] PlayerInitializer が取得できません。", this);
//                return;
//            }

//            MusicSyncInitializer musicSyncInitializer = FindFirstObjectByType<MusicSyncInitializer>();
//            MusicSyncView musicSyncView = FindAnyObjectByType<MusicSyncView>();
//            if (musicSyncInitializer?.MusicSyncService == null || musicSyncView?.MusicSyncState == null)
//            {
//                Debug.LogError("[BossTestInitializer] MusicSync の取得に失敗しました。", this);
//                return;
//            }

//            IMusicSyncService musicSyncService = musicSyncInitializer.MusicSyncService;
//            MusicSyncState musicSyncState = musicSyncView.MusicSyncState;

//            // ターゲット管理。IngameComposition が ServiceLocator に公開した
//            // 共有インスタンスを使うことで、プレイヤーのロックオン候補にボスが入る。
//            // 取得できない（ボス単独シーン等）場合のみ単独生成にフォールバックする。
//            TargetSystemController targetSystemController = ServiceLocator.GetInstance<TargetSystemController>();
//            if (targetSystemController == null)
//            {
//                ITargetSystemViewModel targetSystemViewModel = new TargetingSystem();
//                TargetEntityRegistry targetEntityRegistry = new();
//                targetSystemController = new TargetSystemController(targetSystemViewModel, targetEntityRegistry);
//            }

//            if (_enemyPools != null)
//            {
//                _enemyPools.InitializeShellOnly();
//            }

//            // ボス初期化。attackControllerGenerator はボスでは未使用のため null。
//            _boss.Initialize(
//                playerInitializer.transform,
//                playerInitializer.PlayerEntity,
//                musicSyncState,
//                musicSyncService,
//                targetSystemController,
//                null,
//                _enemyPools,
//                _ => { });

//            Vector3 spawnPos = _spawnPoint != null ? _spawnPoint.position : transform.position;
//            _boss.Activate(spawnPos, () => { });
//            _boss.StartGameplay();

//            Debug.Log("[BossTestInitializer] ボスを起動しました。");
//        }

//        /// <summary>
//        ///     ServiceLocator から指定サービスが取得できるまで待つ。
//        /// </summary>
//        private async Awaitable<T> WaitServiceAsync<T>() where T : class
//        {
//            T instance = ServiceLocator.GetInstance<T>();
//            int retry = 0;
//            while (instance == null && retry < 60)
//            {
//                await Awaitable.NextFrameAsync(destroyCancellationToken);
//                instance = ServiceLocator.GetInstance<T>();
//                retry++;
//            }
//            return instance;
//        }
//    }
//}
