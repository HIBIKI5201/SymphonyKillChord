using System;
using R3;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public class MusicSyncView : MonoBehaviour
    {
        public MusicSyncViewModel MusicSyncViewModel => _musicSyncViewModel;

#if UNITY_EDITOR
        [SerializeField] private int _testBpm;
#endif

        private MusicPlayer _mp;
        private MusicViewModel _musicViewModel;
        private MusicSyncViewModel _musicSyncViewModel;

        public void Bind(
            MusicPlayer musicPlayer,
            MusicSyncViewModel syncViewModel)
        {
            _mp = musicPlayer;
            _musicViewModel = _mp.MusicVM;
            _musicViewModel.CueName.Subscribe(PlayBgm).RegisterTo(destroyCancellationToken);
            _musicSyncViewModel = syncViewModel;
        }

        private void Update()
        {
            if (_mp == null || _musicSyncViewModel.Bpm <= 0 || _musicSyncViewModel.BeatLength <= 0) return;

            _musicSyncViewModel.Update(_mp.Time);

            _musicSyncViewModel.AccurateBeat = _mp.Time / _musicSyncViewModel.BeatLength;
            _musicSyncViewModel.NearestBeat = (int)Math.Round(_musicSyncViewModel.AccurateBeat);
            _musicSyncViewModel.CurrentBeat = (int)Math.Floor(_musicSyncViewModel.AccurateBeat);
        }

        private void PlayBgm(string cueName)
        {
#if UNITY_EDITOR
            _musicSyncViewModel.Bpm = _testBpm; //TODO : cueNameを引数にデータベースからBPMを取得するように変更
#endif
            _musicSyncViewModel.BeatLength = 60000d / _musicSyncViewModel.Bpm;
        }
    }
}