using KillChord.Runtime.Composition.InGame.Music;
using KillChord.Runtime.View.Persistent.Music;
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

        private MusicPlayer _musicPlayer;

        private void Start()
        {
            _musicPlayer = ServiceLocator.GetInstance<MusicPlayer>();
            
            _camerasystemInitializer.Initialize();
            ServiceInjector.Inject(_skillInitializer);
            _skillInitializer.Initialize();
        }
    }
}