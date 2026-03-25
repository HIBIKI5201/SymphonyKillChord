using CriWare;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public class MusicSyncView : MonoBehaviour
    {
        private MusicPlayer _mp;

        public void Bind(MusicPlayer musicPlayer)
        {
            _mp = musicPlayer;
        }

        private void PlayBgm(string cueName)
        {
            _mp.PlayBgm(cueName);
        }

        private void PlayBgm(string cueName, int bpm)
        {
            _mp.PlayBgm(cueName, bpm);
        }

        private void Update()
        {
        }

#if UNITY_EDITOR
        [SerializeField] private string cueName;

        [ContextMenu(nameof(PlayBgm))]
        public void PlayBgm()
        {
            PlayBgm(cueName);
        }
#endif
    }
}