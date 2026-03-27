using KillChord.Runtime.View;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    public class MusicSyncInitializer : MonoBehaviour
    {
        [SerializeField] private string _cueName;
        [SerializeField] private MusicSyncView _musicSyncView;
        [SerializeField] private InputComposition _composition;

        private void Start()
        {
            _composition.GetInputMapController.EnableOnly(InputMapNames.InGame);
            var player = FindFirstObjectByType<MusicPlayer>().GetComponent<MusicPlayer>();
            _musicSyncView.Bind(
                player,
                new(),
                _composition.GetInputView
            );
            _musicSyncView.PlayBgm(_cueName);
        }
    }
}