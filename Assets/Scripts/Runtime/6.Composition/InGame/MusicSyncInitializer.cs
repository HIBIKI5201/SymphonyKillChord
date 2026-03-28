using KillChord.Runtime.Adaptor;
using KillChord.Runtime.View;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    public class MusicSyncInitializer : MonoBehaviour
    {
        [SerializeField] private MusicSyncView _musicSyncView;
        [SerializeField] private string _cue;

        public MusicSyncController _musicSyncViewModel;

        private void Start()
        {
            MusicSyncViewModel msvm = new();
            var mp = FindFirstObjectByType<MusicPlayer>();
            _musicSyncView.Bind(
                mp,
                msvm
            );

            mp.MusicVM.UpdateMusicCue(_cue);
            _musicSyncViewModel = new(msvm);
        }
    }
}