using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Application;
using KillChord.Runtime.Composition;
using KillChord.Runtime.View;
using UnityEngine;

namespace KillChord.Develop
{
    public class EnemyTestMusicSyncInitializer : MonoBehaviour
    {
        [SerializeField] private MusicSyncView _musicSyncView;
        [SerializeField] private string _testCue;
        [SerializeField] private int _testBpm;

        [SerializeField] private EnemyTestSpawner _testSpawner;

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
            _testSpawner.Initialize(msvm, MusicSyncService);
        }
    }
}