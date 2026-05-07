using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.View.Persistent.Music;
using R3;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Music
{
    /// <summary>
    ///     音楽との同期タイミングを管理するViewクラス。
    /// </summary>
    public class MusicSyncView : MonoBehaviour
    {
        public MusicSyncState MusicSyncState => _musicSyncState;

        public void Bind(
            MusicPlayer musicPlayer,
            MusicSyncState musicSyncState,
            MusicSyncController musicSyncController,
            int testBpm)
        {
            _musicPlayer = musicPlayer;
            _musicSyncState = musicSyncState;
            _musicSyncController = musicSyncController;
            _musicViewModel = _musicPlayer.MusicVM;

            _musicViewModel.CueName
                .Subscribe(PlayBgm)
                .RegisterTo(destroyCancellationToken);
        }

        private int _testBpm;
        private MusicPlayer _musicPlayer;
        private MusicViewModel _musicViewModel;
        private MusicSyncState _musicSyncState;
        private MusicSyncController _musicSyncController;

        private void Update()
        {
            if (_musicPlayer == null
                || _musicSyncState == null
                || _musicSyncController == null
                || _musicSyncState.Bpm <= 0
                || _musicSyncState.BeatLength <= 0) return;

            _musicSyncController.Tick(_musicPlayer.Time);
        }

        private void PlayBgm(string cueName)
        {
            _musicSyncState.SetBpm(_testBpm);
        }
    }
}