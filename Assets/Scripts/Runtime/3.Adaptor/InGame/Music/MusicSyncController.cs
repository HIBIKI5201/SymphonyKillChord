using KillChord.Runtime.Adaptor.Persistent.Environment;
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
        /// <param name="environmentSettingsViewModel">
        ///     リズム判定オフセットを公開する環境設定ViewModel。
        ///     未指定の場合はオフセット0秒として扱う。
        /// </param>
        public MusicSyncController(
            MusicSyncState musicSyncState,
            IMusicSyncService musicSyncService,
            IEnvironmentSettingsViewModel environmentSettingsViewModel = null)
        {
            _musicSyncState = musicSyncState;
            _musicSyncService = musicSyncService;
            _environmentSettingsViewModel = environmentSettingsViewModel;
        }

        /// <summary>
        ///     毎フレームの更新処理を行う。
        ///     ユーザー設定のリズム判定オフセットを再生時間へ加算してから同期状態・判定処理へ渡す。
        /// </summary>
        /// <param name="playTime"> 現在の再生時間。 </param>
        public void Tick(double playTime)
        {
            double rhythmOffsetSeconds = _environmentSettingsViewModel?.RhythmOffsetSeconds.CurrentValue ?? 0d;
            double adjustedPlayTime = playTime + rhythmOffsetSeconds;

            _musicSyncState.UpdatePlayTime(adjustedPlayTime);
            _musicSyncService.Update(adjustedPlayTime);
        }

        /// <summary>
        ///     音源の停止・切り替えに合わせて入力とゲージ基準を破棄する。
        /// </summary>
        public void ResetPlayback()
        {
            _musicSyncService.ResetPlayback();
        }

        private readonly MusicSyncState _musicSyncState;
        private readonly IMusicSyncService _musicSyncService;
        private readonly IEnvironmentSettingsViewModel _environmentSettingsViewModel;
    }
}