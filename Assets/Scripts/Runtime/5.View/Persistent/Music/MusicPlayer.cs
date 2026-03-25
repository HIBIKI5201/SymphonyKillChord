using CriWare;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public class MusicPlayer : MonoBehaviour
    {
        private CriAtomSource _cri;

        private CriAtomExPlayback _playback;

        private int _bpm;
        private string _cueName;

        public void Awake()
        {
            _cri = GetComponent<CriAtomSource>();
        }

        public void PlayBgm(string cueName)
        {
            if (cueName == _cueName) return;
            StopBgm();
            _cueName = cueName;
            _cri.cueName = cueName;
            _playback = _cri.Play();
        }

        public void StopBgm()
        {
            _playback.Stop();
            _bpm = -1;
            _cueName = string.Empty;
        }

        public void PlayBgm(string cueName, int bpm)
        {
            PlayBgm(cueName);
            _bpm = bpm;
        }

        public double GetPlayTime()
        {
            return _playback.time;
        }

        public int GetBpm()
        {
            return _bpm;
        }
    }
}