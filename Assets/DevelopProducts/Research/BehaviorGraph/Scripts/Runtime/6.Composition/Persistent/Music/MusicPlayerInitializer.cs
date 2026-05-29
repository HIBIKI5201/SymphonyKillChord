using DevelopProducts.BehaviorGraph.Runtime.View.Persistent.Music;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.Composition.Persistent.Music
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