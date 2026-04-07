using KillChord.Runtime.View;
using KillChord.Runtime.View.InGame.Camera;
using KillChord.Runtime.View.InGame.Music;
using KillChord.Runtime.View.InGame.Player;
using KillChord.Runtime.View.Persistent.Music;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    public class IngameComposition : MonoBehaviour
    {
        [SerializeField] private PlayerView _playerView;
        [SerializeField] private PlayerAttackInputView _playerAttackInputView;
        [SerializeField] private AttackResultView _attackResultView;
        [SerializeField] private CameraSystemView _cameraSystemView;
        [SerializeField] private MusicSyncView _musicSyncView;

        private MusicPlayer _musicPlayer;

        private void Start()
        {
            _musicPlayer = ServiceLocator.GetInstance<MusicPlayer>();
        }
    }
}