using KillChord.Runtime.Composition.InGame.Music;
using KillChord.Runtime.View;
using KillChord.Runtime.View.Persistent.Music;
using SymphonyFrameWork.Attribute;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    [DefaultExecutionOrder(-100)]
    public class IngameComposition : MonoBehaviour
    {
        [SerializeField] private MusicSyncInitializer _musicSyncInitializer;
        [SerializeField] private CameraSystemInitializer _camerasystemInitializer;
        [SerializeField] private IngameSceneView _ingameSceneView;

        [SerializeField, SceneNameSelector] private string _backgroundSceneName;

        private PlayerInitializer _playerInitializer;
        private SkillInitializer _skillInitializer;
        private MusicPlayer _musicPlayer;

        private async void Start()
        {
            await _ingameSceneView.LoadScene(_backgroundSceneName);
            _skillInitializer = ServiceLocator.GetInstance<SkillInitializer>();
            _playerInitializer = ServiceLocator.GetInstance<PlayerInitializer>();

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

            // 初期化順序の実行
            _musicSyncInitializer.Initialize();

            _playerInitializer.Initialize();

            ServiceInjector.Inject(_skillInitializer);
            _skillInitializer.Initialize();

            _camerasystemInitializer.Initialize();
        }
    }
}