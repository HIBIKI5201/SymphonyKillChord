using KillChord.Runtime.View.Persistent.Music;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.Persistent.Music
{
    [RequireComponent(typeof(MusicPlayer))]
    public class MusicPlayerInitializer : MonoBehaviour
    {
        private void Awake()
        {
            MusicPlayer musicPlayer = GetComponent<MusicPlayer>();
            MusicViewModel musicViewModel = new MusicViewModel();
            musicPlayer.Bind(musicViewModel);
            ServiceLocator.RegisterInstance(musicPlayer);
        }
    }
}
