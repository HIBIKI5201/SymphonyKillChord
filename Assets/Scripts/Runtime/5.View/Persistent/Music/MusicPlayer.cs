using CriWare;
using R3;
using SymphonyFrameWork.Debugger.HUD;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public class MusicPlayer : MonoBehaviour
    {
        public MusicViewModel MusicVM => _musicVm;
        public double Time => _playback.time;
        public string CueName => _cueName;

        private CriAtomSource _cri;
        private CriAtomExPlayback _playback;
        private MusicViewModel _musicVm;


        private string _cueName;

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
            if (cueName == _cueName) return;
            StopBgm();
            _cueName = cueName;
            _cri.cueName = cueName;
            _playback = _cri.Play();
            Debug.Log($"cueName : {cueName}");
            SymphonyDebugHUD.AddText(() => _playback.time.ToString());
        }

        public void StopBgm()
        {
            _playback.Stop();
            _cueName = string.Empty;
        }
    }
}