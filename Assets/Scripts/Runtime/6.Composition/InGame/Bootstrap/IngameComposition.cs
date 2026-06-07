using KillChord.Runtime.Application.InGame.Camera.Target;
using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Composition.InGame.Camera;
using KillChord.Runtime.Composition.InGame.Enemy;
using KillChord.Runtime.Composition.InGame.Mission;
using KillChord.Runtime.Composition.InGame.Music;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.Composition.InGame.Sequence;
using KillChord.Runtime.Composition.Persistent.Input;
using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.View.InGame.Enemy;
using KillChord.Runtime.View.InGame.Player;
using KillChord.Runtime.View.InGame.Scene;
using KillChord.Runtime.View.InGame.Sequence;
using KillChord.Runtime.View.Persistent.Input;
using KillChord.Runtime.View.Persistent.Music;
using SymphonyFrameWork.Attribute;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Bootstrap
{
    /// <summary>
    ///    インゲームシーンの全体的な初期化と構成を担当するクラス。
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class IngameComposition : MonoBehaviour
    {
        [SerializeField] private MusicSyncInitializer _musicSyncInitializer;
        [SerializeField] private CameraSystemInitializer _camerasystemInitializer;
        [SerializeField] private IngameSceneView _ingameSceneView;
        [SerializeField] private EnemyInfantrySpawner _enemyInfantryTestSpawner;
        [SerializeField] private EnemyArtillerySpawner _enemyArtilleryTestSpawner;
        [SerializeField] private InGameMissionInitializer _inGameMissionInitializer;
        [SerializeField] private MobileInput _mobileInput;
        [SerializeField] private RhythmGuideInitializer _rhythmGuideInitializer;
        [SerializeField] private NecrodancerRhythmGuideInitializer _necrodancerRhythmGuideInitializer;
        [SerializeField] private BPMRhythmGuideInitializer _bpmRhythmGuideInitializer;
        [SerializeField, SceneNameSelector] private string _backgroundSceneName;
        [SerializeField] private EnemyPools _enemyPools;
        [SerializeField] private EnemyInitializer _enemyInitializer;
        [SerializeField] private EnemySpawnPositionSearcher _enemySpawnPositionSearcher;
        [SerializeField] private StageSequenceView _stageSequenceView;
        [SerializeField] private StageResultUIView _stageResultUIView;
        [SerializeField] private InGamePlayDirector _inGamePlayDirector;

        private PlayerInitializer _playerInitializer;
        private MusicPlayer _musicPlayer;
        private InGameSequenceDirector _inGameSequenceDirector;
        private MissionRuntimeService _missionruntimeService;
        private bool _isEnding = false;

        private async void Start()
        {
            await _ingameSceneView.LoadScene(_backgroundSceneName);

            if (!await TryInitializeAsync())
            {
                Debug.LogError("[IngameComposition] 初期化に失敗しました。");
                return;
            }

            _missionruntimeService = ServiceLocator.GetInstance<MissionRuntimeService>();
            if (_missionruntimeService != null)
            {
                _missionruntimeService.OnMissionFinished += HandleMissionFinished;

            }

            await _inGameSequenceDirector.StartAsync(destroyCancellationToken);
        }

        private void OnDestroy()
        {
            if (_missionruntimeService != null)
            {
                _missionruntimeService.OnMissionFinished -= HandleMissionFinished;
            }
        }

        /// <summary>
        ///     シーン内の必要なコンポーネントやサービスを非同期に初期化する。
        /// </summary>
        /// <returns> 初期化が成功したかどうかを示す値。 </returns>
        private async Awaitable<bool> TryInitializeAsync()
        {
            _playerInitializer = ServiceLocator.GetInstance<PlayerInitializer>();

            if (_playerInitializer == null)
            {
                Debug.LogError("[IngameComposition] PlayerInitializer の取得に失敗しました。");
                return false;
            }

            var stageSceneI = await ServiceLocator.GetInstanceAsync<IStageSceneInstance>();
            Debug.Log(
                $"stageSceneI {stageSceneI != null}  PlayerT{stageSceneI.PlayerTransform != null} Skill{stageSceneI.SkillInitializer}");

            if (!await WaitMusicPlayerAsync())
            {
                return false;
            }

            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[IngameComposition] MainCamera が見つかりません。");
                return false;
            }

            if (!ValidateInitialization())
            {
                return false;
            }

            TargetManager targetManager = new();
            TargetEntityRegistry targetEntityRegistry = new();

            // 初期化順序の実行
            _musicSyncInitializer.Initialize();
            _inGameMissionInitializer.Initialize();
            _stageResultUIView.Initialize();
            var inputC = ServiceLocator.GetInstance<InputComposition>();
            if (inputC == null)
            {
                Debug.LogError("[IngameComposition] InputComposition の取得に失敗しました。", this);
                return false;
            }

            inputC.GetInputMapController.EnableOnly(InputMapNames.Common);

#if UNITY_ANDROID
            _camerasystemInitializer.Initialize(targetManager, targetEntityRegistry);
            _mobileInput.Initialize(inputC.GetInputView);
#else
            _camerasystemInitializer.Initialize(targetManager, targetEntityRegistry);
            Cursor.lockState = CursorLockMode.Locked;
#endif

            _playerInitializer.Initialize(targetManager, targetEntityRegistry, inputC);

            PlayerView playerView = FindFirstObjectByType<PlayerView>();

            if (playerView == null)
            {
                Debug.LogError("[IngameComposition] PlayerView が見つかりません。");
                return false;
            }

            _inGamePlayDirector.AddGamePlayControllable(playerView);

            // ステージに事前配置されている敵の情報
            AssignedEnemyManager assignedEnemyManager = FindFirstObjectByType<AssignedEnemyManager>();
            if (assignedEnemyManager == null)
            {
                Debug.LogWarning("[IngameComposition] 敵の事前配置情報がありません。");
            }
            else
            {
                if (assignedEnemyManager.Infantries == null
                    || assignedEnemyManager.Infantries.Length == 0)
                {
                    Debug.LogWarning("[IngameComposition] 歩兵の事前配置情報がありません。");
                }
                if (assignedEnemyManager.Artillery == null
                    || assignedEnemyManager.Artillery.Length == 0)
                {
                    Debug.LogWarning("[IngameComposition] 砲兵の事前配置情報がありません。");
                }
            }

            // 敵生成関連
            _enemyPools.Initialize();
            _enemyInitializer.Initialize(targetManager, targetEntityRegistry, _enemyPools);

            _enemySpawnPositionSearcher.Initialize(mainCamera, _playerInitializer.transform);
            _enemyInfantryTestSpawner.Initialize(assignedEnemyManager?.Infantries);
            _enemyArtilleryTestSpawner.Initialize(assignedEnemyManager?.Artillery);

            _rhythmGuideInitializer.Initialize();
            _necrodancerRhythmGuideInitializer.Initialize();
            _bpmRhythmGuideInitializer.Initialize();

            _inGameSequenceDirector = new InGameSequenceDirector(
                _stageSequenceView,
                _stageResultUIView,
                _inGamePlayDirector
                );
            _inGamePlayDirector.StopGameplay();

            return true;
        }

        /// <summary>
        ///    MusicPlayer が ServiceLocator から利用可能になるまで待機する。
        /// </summary>
        /// <returns> MusicPlayer が利用可能になったかどうかを示す値。 </returns>
        private async Awaitable<bool> WaitMusicPlayerAsync()
        {
            // 常駐サービスの取得を確実にするため、取得できるまで待機する
            _musicPlayer = ServiceLocator.GetInstance<MusicPlayer>();

            int retryCount = 0;
            while (_musicPlayer == null && retryCount < 20)
            {
                await Awaitable.NextFrameAsync(destroyCancellationToken);
                _musicPlayer = ServiceLocator.GetInstance<MusicPlayer>();
                retryCount++;
            }

            if (_musicPlayer != null)
            {
                return true;
            }

            Debug.LogError("[IngameComposition] MusicPlayer の取得に失敗しました。常駐シーンがロードされているか確認してください。");
            return false;
        }

        /// <summary>
        ///     シーン内の必要なコンポーネントがすべて設定されているかを検証する。
        /// </summary>
        /// <returns> 初期化が有効かどうかを示す値。 </returns>
        private bool ValidateInitialization()
        {
            if (_musicSyncInitializer == null)
            {
                Debug.LogError("[IngameComposition] MusicSyncInitializerの参照が未設定です。", this);
                return false;
            }
            if (_camerasystemInitializer == null)
            {
                Debug.LogError("[IngameComposition] CameraSystemInitializerの参照が未設定です。", this);
                return false;
            }
            if (_ingameSceneView == null)
            {
                Debug.LogError("[IngameComposition] IngameSceneViewの参照が未設定です。", this);
                return false;
            }
            if (_enemyPools == null)
            {
                Debug.LogError("[IngameComposition] EnemyPoolsの参照が未設定です。", this);
                return false;
            }
            if (_enemyInitializer == null)
            {
                Debug.LogError("[IngameComposition] EnemyInitializerの参照が未設定です。", this);
                return false;
            }
            if (_enemySpawnPositionSearcher == null)
            {
                Debug.LogError("[IngameComposition] EnemySpawnPositionSearcherの参照が未設定です。", this);
                return false;
            }
            if (_enemyInfantryTestSpawner == null)
            {
                Debug.LogError("[IngameComposition] EnemyInfantryTestSpawnerの参照が未設定です。", this);
                return false;
            }
            if (_enemyArtilleryTestSpawner == null)
            {
                Debug.LogError("[IngameComposition] EnemyArtilleryTestSpawnerの参照が未設定です。", this);
                return false;
            }
            if (_rhythmGuideInitializer == null)
            {
                Debug.LogError("[IngameComposition] RhythmGuideInitializerの参照が未設定です。", this);
                return false;
            }
            if (_necrodancerRhythmGuideInitializer == null)
            {
                Debug.LogError("[IngameComposition] NecrodancerRhythmGuideInitializerの参照が未設定です。", this);
                return false;
            }
            if (_bpmRhythmGuideInitializer == null)
            {
                Debug.LogError("[IngameComposition] BPMRhythmGuideInitializerの参照が未設定です。", this);
                return false;
            }
            if (_stageSequenceView == null)
            {
                Debug.LogError("[IngameComposition] StageSequenceViewの参照が未設定です。", this);
                return false;
            }
            if (_inGamePlayDirector == null)
            {
                Debug.LogError("[IngameComposition] InGamePlayDirectorの参照が未設定です。", this);
                return false;
            }
            if (_stageResultUIView == null)
            {
                Debug.LogError("[IngameComposition] StageResultUIViewの参照が未設定です。", this);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     ミッションの終了イベントを処理する。
        ///     ミッションの終了理由に応じて、クリアシーケンスまたはゲームオーバーシーケンスを開始する。
        /// </summary>
        /// <param name="reason">ミッションの終了理由。</param>
        private async void HandleMissionFinished(MissionEndReason reason)
        {
            if (_isEnding)
            {
                return;
            }

            _isEnding = true;

            Debug.Log($"Mission Finished: {reason}");
            switch (reason)
            {
                case MissionEndReason.Clear:
                    await _inGameSequenceDirector.ClearAsync(destroyCancellationToken);
                    break;
                case MissionEndReason.Fail:
                    await _inGameSequenceDirector.GameOverAsync(destroyCancellationToken);
                    break;
            }
        }
    }
}
