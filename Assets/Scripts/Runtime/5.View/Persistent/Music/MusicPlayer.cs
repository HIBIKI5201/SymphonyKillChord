using CriWare;
using R3;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public class MusicPlayer : MonoBehaviour
    {
        public MusicViewModel MusicVM => _musicVm;
        public double Time => _playback.time;

        private CriAtomSource _cri;
        private CriAtomExPlayback _playback;
        private MusicViewModel _musicVm;

        public void Bind(MusicViewModel musicViewModel)
        {
            _musicVm = musicViewModel;
            musicViewModel.CueName.Subscribe(PlayBgm).RegisterTo(destroyCancellationToken);
        }

        public void Awake()
        {
            _cri = GetComponent<CriAtomSource>();
            Bind(new());
        }

        public void PlayBgm(string cueName)
        {
            if (cueName == _cri.cueName || cueName == string.Empty) return;
            StopBgm();
            _cri.cueName = cueName;
            _playback = _cri.Play();
        }

        public void StopBgm()
        {
            _playback.Stop();
            _cri.cueName = string.Empty;
        }
    }
}