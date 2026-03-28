using KillChord.Runtime.View;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    public class MusicSyncInitializer : MonoBehaviour
    {
        [SerializeField] private MusicSyncView _musicSyncView;
        [SerializeField] private string _cue;

        private void Start()
        {
            var mp = FindFirstObjectByType<MusicPlayer>();
            var _composition = FindFirstObjectByType<PlayerInputView>();
            _musicSyncView.Bind(
                mp,
                new(),
                _composition
            );
            
            mp.MusicVM.UpdateMusicCue(_cue);
        }
    }
}