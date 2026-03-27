using KillChord.Runtime.View;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    public class MusicSyncInitializer : MonoBehaviour
    {
        [SerializeField] private MusicSyncView _musicSyncView;
        [SerializeField] private InputComposition _composition;
        [SerializeField] private string _cue;

        private void Start()
        {
            var mp = FindFirstObjectByType<MusicPlayer>();
            _composition.GetInputMapController.EnableOnly(InputMapNames.InGame);
            _musicSyncView.Bind(
                mp,
                new(),
                _composition.GetInputView
            );
            
            mp.MusicVM.UpdateMusicCue(_cue);
        }
    }
}