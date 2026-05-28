using KillChord.Runtime.Application.InGame.Camera.Target;
using KillChord.Runtime.Composition.InGame.Camera;
using KillChord.Runtime.Composition.InGame.Enemy;
using KillChord.Runtime.Composition.InGame.Mission;
using KillChord.Runtime.Composition.InGame.Music;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.Composition.Persistent.Input;
using KillChord.Runtime.View.InGame.Enemy;
using KillChord.Runtime.View.InGame.Scene;
using KillChord.Runtime.View.Persistent.Input;
using KillChord.Runtime.View.Persistent.Music;
using SymphonyFrameWork.Attribute;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Bootstrap
{
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
        [SerializeField, SceneNameSelector] private string _backgroundSceneName;
        [SerializeField] private EnemyPools _enemyPools;
        [SerializeField] private EnemyInitializer _enemyInitializer;
        [SerializeField] private EnemySpawnPositionSearcher _enemySpawnPositionSearcher;

        private PlayerInitializer _playerInitializer;
        private MusicPlayer _musicPlayer;

        private async void Start()
        {
            await _ingameSceneView.LoadScene(_backgroundSceneName);

            _playerInitializer = ServiceLocator.GetInstance<PlayerInitializer>();
            if( _playerInitializer == null)
            {
                Debug.LogError("[IngameComposition] PlayerInitializer の取得に失敗しました。");
                return;
            }
            var stageSceneI = await ServiceLocator.GetInstanceAsync<IStageSceneInstance>();
            Debug.Log(
                $"stageSceneI {stageSceneI != null}  PlayerT{stageSceneI.PlayerTransform != null} Skill{stageSceneI.SkillInitializer}");

            // 常駐サービスの取得を確実にするため、取得できるまで待機する
            _musicPlayer = ServiceLocator.GetInstance<MusicPlayer>();

            int retryCount = 0;
            while (_musicPlayer == null && retryCount < 20)
            {
                await System.Threading.Tasks.Task.Delay(100);
                _musicPlayer = ServiceLocator.GetInstance<MusicPlayer>();
                retryCount++;
            }

            if (_musicPlayer == null)
            {
                Debug.LogError("[IngameComposition] MusicPlayer の取得に失敗しました。常駐シーンがロードされているか確認してください。");
                return;
            }

            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[IngameComposition] MainCamera が見つかりません。");
                return;
            }
            if (_enemyPools == null)
            {
                Debug.LogError("[IngameComposition] EnemyPoolsの参照が未設定です。");
                return;
            }
            if (_enemyInitializer == null)
            {
                Debug.LogError("[IngameComposition] EnemyInitializerの参照が未設定です。");
                return;
            }
            if (_enemySpawnPositionSearcher == null)
            {
                Debug.LogError("[IngameComposition] EnemySpawnPositionSearcherの参照が未設定です。");
                return;
            }
            if (_enemyInfantryTestSpawner == null)
            {
                Debug.LogError("[IngameComposition] EnemyInfantryTestSpawnerの参照が未設定です。");
                return;
            }
            if (_enemyArtilleryTestSpawner == null)
            {
                Debug.LogError("[IngameComposition] EnemyArtilleryTestSpawnerの参照が未設定です。");
                return;
            }

            TargetManager targetManager = new();
            TargetEntityRegistry targetEntityRegistry = new();

            // 初期化順序の実行
            _musicSyncInitializer.Initialize();
            _inGameMissionInitializer.Initialize();

            var inputC = ServiceLocator.GetInstance<InputComposition>();
            inputC.GetInputMapController.EnableOnly(InputMapNames.InGame);
#if UNITY_ANDROID
            _camerasystemInitializer.Initialize(targetManager, targetEntityRegistry);
            _mobileInput.Initialize(inputC.GetInputView);
#else
            _camerasystemInitializer.Initialize(targetManager, targetEntityRegistry);
            Cursor.lockState = CursorLockMode.Locked;
#endif

            _playerInitializer.Initialize(targetManager, targetEntityRegistry, inputC);

            // ステージに事前配置されている敵の情報
            AssignedEnemyManager assignedEnemyManager = FindFirstObjectByType<AssignedEnemyManager>();
            if(assignedEnemyManager == null)
            {
                Debug.LogWarning("[IngameComposition] 敵の事前配置情報がありません。");
            }
            else
            {
                if (assignedEnemyManager.Infantries == null || assignedEnemyManager.Infantries.Length == 0)
                {
                    Debug.LogWarning("[IngameComposition] 歩兵の事前配置情報がありません。");
                }
                if (assignedEnemyManager.Artillery == null || assignedEnemyManager.Artillery.Length == 0)
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

            // リズムガイドUI
            _rhythmGuideInitializer.Initialize();
        }
    }
}
