using KillChord.Runtime.Composition.InGame.Music;
using KillChord.Runtime.View;
using KillChord.Runtime.View.Persistent.Music;
using SymphonyFrameWork.Attribute;
using SymphonyFrameWork.System.SceneLoad;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    public class IngameComposition : MonoBehaviour
    {
        [SerializeField] private PlayerInitializer _playerInitializer;
        [SerializeField] private MusicSyncInitializer _musicSyncInitializer;
        [SerializeField] private CameraSystemInitializer _camerasystemInitializer;
        [SerializeField] private SkillInitializer _skillInitializer;
        [SerializeField] private IngameSceneView _ingameSceneView;

        [SerializeField, SceneNameSelector] private string _backgroundSceneName;

        private MusicPlayer _musicPlayer;

        private async void Start()
        {
            await _ingameSceneView.LoadScene(_backgroundSceneName);
            _musicPlayer = ServiceLocator.GetInstance<MusicPlayer>();

            _camerasystemInitializer.Initialize();
            ServiceInjector.Inject(_skillInitializer);
            _skillInitializer.Initialize();
        }
    }
}