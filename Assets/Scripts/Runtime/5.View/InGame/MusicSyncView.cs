using UnityEngine;

namespace KillChord.Runtime.View
{
    public class MusicSyncView : MonoBehaviour
    {
        public int Bpm => _bpm;

        private MusicPlayer _mp;
        private int _bpm;

        public void Bind(MusicPlayer musicPlayer)
        {
            _mp = musicPlayer;
        }

        private void PlayBgm(string cueName, int bpm)
        {
            _mp.PlayBgm(cueName);
            _bpm = bpm;
        }

#if UNITY_EDITOR
        [SerializeField] private string cueName;

        [ContextMenu(nameof(PlayBgm))]
        public void PlayBgm()
        {
            PlayBgm(cueName, 1);
        }
#endif
    }
}