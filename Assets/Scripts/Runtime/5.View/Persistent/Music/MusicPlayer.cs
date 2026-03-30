using CriWare;
using R3;
using UnityEngine;

namespace KillChord.Runtime.View
{
    [RequireComponent(typeof(CriAtomSource))]
    public class MusicPlayer : MonoBehaviour
    {
        public MusicViewModel MusicVM => _musicVm;

        /// <summary> 現在の曲の累計再生時間を取得 </summary>
        public double Time => _playback.time / 1000d;

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
            if (string.IsNullOrEmpty(cueName) || cueName == _cri.cueName)
            {
                Debug.Log("cueNameが空か元と同じです");
                return;
            }

            StopBgm();
            _cri.cueName = cueName;
            _playback = _cri.Play();
        }

        public void StopBgm()
        {
            _playback.Stop();
            _cri.cueName = string.Empty;
            _musicVm.UpdateMusicCue(string.Empty);
        }

        private CriAtomSource _cri;
        private CriAtomExPlayback _playback;
        private MusicViewModel _musicVm;
    }
}