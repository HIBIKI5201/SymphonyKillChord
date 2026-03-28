using System;
using System.Threading;
using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Utility;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KillChord.Runtime.View
{
    public class MusicSyncView : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private int _testBpm;
#endif

        private MusicPlayer _mp;
        private MusicViewModel _musicViewModel;
        private MusicSyncViewModel _musicSyncViewModel;

        private double _beatLength;
        private double _currentBeat;

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
            if (_mp == null || _musicSyncViewModel.Bpm <= 0) return;

            _musicSyncViewModel.Update(_mp.Time);
            
            _currentBeat = _mp.Time / _beatLength;
            _musicSyncViewModel.NearestBeat = (int)Math.Round(_currentBeat);
            _musicSyncViewModel.CurrentBeat = (int)Math.Floor(_currentBeat);
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