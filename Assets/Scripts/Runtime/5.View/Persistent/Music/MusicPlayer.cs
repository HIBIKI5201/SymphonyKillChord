using CriWare;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public class MusicPlayer : MonoBehaviour
    {
        public double Time => _playback.time;
        public string CueName => _cueName;

        private CriAtomSource _cri;
        private CriAtomExPlayback _playback;


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
            _cueName = string.Empty;
        }
    }
}