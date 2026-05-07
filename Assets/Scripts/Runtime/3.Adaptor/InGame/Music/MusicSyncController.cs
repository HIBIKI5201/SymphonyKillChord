using KillChord.Runtime.Application.InGame.Music;

namespace KillChord.Runtime.Adaptor.InGame.Music
{
    /// <summary>
    ///     音楽との同期タイミングの更新を制御するコントローラークラス。
    /// </summary>
    public class MusicSyncController
    {
        public MusicSyncController(MusicSyncState musicSyncState, IMusicSyncService musicSyncService)
        {
            _musicSyncState = musicSyncState;
            _musicSyncService = musicSyncService;
        }

        public void Tick(double playTime)
        {
            _musicSyncState.UpdatePlayTime(playTime);
            _musicSyncService.Update(playTime);
        }

        private readonly MusicSyncState _musicSyncState;
        private readonly IMusicSyncService _musicSyncService;
    }
}