using R3;
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
    }
}