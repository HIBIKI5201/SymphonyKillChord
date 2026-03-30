using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Application;
using KillChord.Runtime.View;
using KillChord.Runtime.View.InGame.Music;
using KillChord.Runtime.View.Persistent.Music;
using UnityEngine;
namespace KillChord.Runtime.Composition.InGame.Music
{
    public class MusicSyncInitializer : MonoBehaviour
    {
        [SerializeField] private MusicSyncView _musicSyncView;
        [SerializeField] private string _testCue;
        [SerializeField] private int _testBpm;

        public MusicSyncController MusicSyncController;
        public MusicSyncService MusicSyncService;

        private void Start()
        {
            MusicSyncViewModel msvm = new();
            var mp = FindFirstObjectByType<MusicPlayer>();
            _musicSyncView.Bind(
                mp,
                msvm
            );

            mp.MusicVM.UpdateMusicCue(_testCue);
            MusicSyncService = new(new(_testBpm));
            MusicSyncController = new(msvm, MusicSyncService);
        }
    }
}