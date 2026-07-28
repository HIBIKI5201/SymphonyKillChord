using KillChord.Runtime.Application.InGame.Music;

namespace KillChord.Runtime.Adaptor.InGame.Music
{
    /// <summary>
    ///     音楽との同期タイミングの更新を制御するコントローラークラス。
    /// </summary>
    public class MusicSyncController
    {
        /// <summary>
        ///     新しいコントローラーを生成する。
        /// </summary>
        /// <param name="musicSyncState"> 音楽同期状態。 </param>
        /// <param name="musicSyncService"> 音楽同期サービス。 </param>
        public MusicSyncController(MusicSyncState musicSyncState, IMusicSyncService musicSyncService)
        {
            _musicSyncState = musicSyncState;
            _musicSyncService = musicSyncService;
        }

        /// <summary>
        ///     毎フレームの更新処理を行う。
        /// </summary>
        /// <param name="playTime"> 現在の再生時間。 </param>
        public void Tick(double playTime)
        {
            _musicSyncState.UpdatePlayTime(playTime);
            _musicSyncService.Update(playTime);
        }

        private readonly MusicSyncState _musicSyncState;
        private readonly IMusicSyncService _musicSyncService;
    }
}