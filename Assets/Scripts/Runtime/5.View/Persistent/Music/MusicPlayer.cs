using CriWare;
using R3;
using UnityEngine;

namespace KillChord.Runtime.View.Persistent.Music
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
            musicViewModel.CueName.Subscribe(ChangeBgm).RegisterTo(destroyCancellationToken);
        }

        public void Awake()
        {
            _cri = GetComponent<CriAtomSource>();
        }

        private void ChangeBgm(string cueName)
        {
            StopBgm();

            if (string.IsNullOrEmpty(cueName))
            {
                Debug.Log("BGMの再生を停止します。");
                return;
            }

            if (cueName == _cri.cueName)
            {
                Debug.Log("cueNameが元と同じです。");
                return;
            }

            _cri.cueName = cueName;
            _playback = _cri.Play();
        }

        private void StopBgm()
        {
            _playback.Stop();
            _cri.cueName = string.Empty;
        }

        private CriAtomSource _cri;
        private CriAtomExPlayback _playback;
        private MusicViewModel _musicVm;
    }
}