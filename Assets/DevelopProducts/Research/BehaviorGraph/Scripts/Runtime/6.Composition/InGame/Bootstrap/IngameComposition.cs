using DevelopProducts.BehaviorGraph.Runtime.Application;
using DevelopProducts.BehaviorGraph.Runtime.Application.InGame;
using DevelopProducts.BehaviorGraph.Runtime.Composition.InGame.Enemy;
using DevelopProducts.BehaviorGraph.Runtime.Composition.InGame.Music;
using DevelopProducts.BehaviorGraph.Runtime.View;
using DevelopProducts.BehaviorGraph.Runtime.View.Persistent.Music;
using SymphonyFrameWork.Attribute;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.Composition
{
    [DefaultExecutionOrder(-100)]
    public class IngameComposition : MonoBehaviour
    {
        [SerializeField] private MusicSyncInitializer _musicSyncInitializer;
        [SerializeField] private CameraSystemInitializer _camerasystemInitializer;
        //[SerializeField] private IngameSceneView _ingameSceneView;
        [SerializeField] private EnemyTestSpawnerBG _enemyTestSpawner;

        [SerializeField, SceneNameSelector] private string _backgroundSceneName;

        [SerializeField] private PlayerInitializer _playerInitializer;
        [SerializeField] private SkillInitializer _skillInitializer;
        [SerializeField] private MusicPlayer _musicPlayer;

        private async void Start()
        {
            //await _ingameSceneView.LoadScene(_backgroundSceneName);
            //_playerInitializer = ServiceLocator.GetInstance<PlayerInitializer>();
            //var stageSceneI = await ServiceLocator.GetInstanceAsync<IStageSceneInstance>();
            //Debug.Log(
            //    $"stageSceneI {stageSceneI != null}  PlayerT{stageSceneI.PlayerTransform != null} Skill{stageSceneI.SkillInitializer}");
            //_skillInitializer = stageSceneI.SkillInitializer;

            // 常駐サービスの取得を確実にするため、取得できるまで待機する
            //_musicPlayer = ServiceLocator.GetInstance<MusicPlayer>();

            int retryCount = 0;
            while (_musicPlayer == null && retryCount < 20)
            {
                await System.Threading.Tasks.Task.Delay(100);
                //_musicPlayer = ServiceLocator.GetInstance<MusicPlayer>();
                retryCount++;
            }

            if (_musicPlayer == null)
            {
                Debug.LogError("[IngameComposition] MusicPlayer の取得に失敗しました。常駐シーンがロードされているか確認してください。");
                return;
            }
            TargetManager targetManager = new();
            TargetEntityRegistry targetEntityRegistry = new();

            // 初期化順序の実行
            _musicSyncInitializer.Initialize();

            _camerasystemInitializer.Initialize(targetManager, targetEntityRegistry);
            _playerInitializer.Initialize(targetManager, targetEntityRegistry);

            ServiceInjector.Inject(_skillInitializer);
            _skillInitializer.Initialize();

            _enemyTestSpawner.Init();
            Debug.Log("【IngameComposition】End");
        }
    }
}