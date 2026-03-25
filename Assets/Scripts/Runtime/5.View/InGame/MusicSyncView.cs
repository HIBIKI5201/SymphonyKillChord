using System;
using KillChord.Runtime.Adaptor;
using R3;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public class MusicSyncView : MonoBehaviour, IMusicSyncViewModel
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
            _musicViewModel.CueName.Subscribe(PlayBgm).RegisterTo(destroyCancellationToken);
        }

        private void Update()
        {
            if (_mp == null || _bpm <= 0) return;
        }

        private void PlayBgm(string cueName)
        {
            _bpm = _testBpm; //TODO : cueNameを引数にデータベースからBPMを取得するように変更
        }

        private void OnAttack()
        {
            
        }

        /// <summary>
        /// 1~8拍子の中で最も近いものを取得する
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        private int GetNearestSignature(double seconds)
        {
            if (_bpm <= 0) return 4;

            double beatSeconds = 60d / _bpm;
            double barSeconds = beatSeconds * 4d;

            int nearestSignature = 1;
            double minDiff = double.MaxValue;

            for (int i = 1; i <= 8; i++)
            {
                double targetSeconds = barSeconds / i;
                double diff = Math.Abs(seconds - targetSeconds);

                if (diff < minDiff)
                {
                    minDiff = diff;
                    nearestSignature = i;
                }
            }

            return nearestSignature;
        }
    }
}