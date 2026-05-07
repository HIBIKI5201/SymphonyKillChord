using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.View.InGame.Music;
using KillChord.Runtime.View.Persistent.Music;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Music
{
    /// <summary>
    ///     音楽同期機能の初期化を行うクラス。
    /// </summary>
    public class MusicSyncInitializer : MonoBehaviour
    {
        public MusicSyncController MusicSyncController { get; private set; }
        public MusicSyncService MusicSyncService { get; private set; }

        public void Initialize()
        {
            MusicSyncState musicSyncViewState = new();
            var musicPlayer = ServiceLocator.GetInstance<MusicPlayer>();

            MusicSyncService = new MusicSyncService(new RhythmDefinition(_testBpm));
            MusicSyncController = new(musicSyncViewState, MusicSyncService);
            _musicSyncView.Bind(
                musicPlayer,
                musicSyncViewState,
                MusicSyncController,
                _testBpm
            );

            musicPlayer.MusicVM.UpdateMusicCue(_testCue);
            ServiceLocator.RegisterInstance<IMusicSyncService>(MusicSyncService);
        }

        [SerializeField] private MusicSyncView _musicSyncView;
        [SerializeField] private string _testCue;
        [SerializeField] private int _testBpm;
    }
}