using System;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public class MusicSyncView : MonoBehaviour
    {
        public int Bpm => _bpm;

#if UNITY_EDITOR
        [SerializeField] private int _testBpm;
#endif

        private MusicPlayer _mp;
        private MusicViewModel _musicViewModel;
        private int _bpm;

        public void Bind(MusicPlayer musicPlayer, MusicViewModel musicViewModel)
        {
            _mp = musicPlayer;
            _musicViewModel = musicViewModel;
        }

        private void Update()
        {
        }

        private void PlayBgm(string cueName)
        {
            _bpm = _testBpm;
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