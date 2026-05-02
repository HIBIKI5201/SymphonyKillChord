using KillChord.Runtime.View.InGame.Music;
using KillChord.Runtime.View.Persistent.Music;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.Persistent.Music
{
    /// <summary>
    ///     音楽再生機能の初期化を行うクラス。
    /// </summary>
    [RequireComponent(typeof(MusicPlayer))]
    public class MusicPlayerInitializer : MonoBehaviour
    {
        private void Awake()
        {
            MusicPlayer musicPlayer = GetComponent<MusicPlayer>();
            MusicViewModel musicViewModel = new MusicViewModel();
            musicPlayer.Bind(musicViewModel);
            musicPlayer.Initialize();
            ServiceLocator.RegisterInstance(musicPlayer);
        }
    }
}