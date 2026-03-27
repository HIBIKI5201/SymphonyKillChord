using KillChord.Runtime.View;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    public class MusicSyncInitializer : MonoBehaviour
    {
        [SerializeField] private MusicSyncView _musicSyncView;
        [SerializeField] private InputComposition _composition;

        private void Start()
        {
            _composition.GetInputMapController.EnableOnly(InputMapNames.InGame);
            _musicSyncView.Bind(
                FindFirstObjectByType<MusicPlayer>().GetComponent<MusicPlayer>(),
                new(),
                new(),
                _composition.GetInputView
            );
        }
    }
}