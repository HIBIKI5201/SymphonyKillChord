using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Application.InGame.Music;

namespace KillChord.Runtime.Composition.InGame.Music
{
    /// <summary>
    ///     音楽同期モジュールの公開物を保持するContainerです。
    /// </summary>
    public sealed class MusicSyncModuleContainer
    {
        /// <summary>
        ///     Containerを生成します。
        /// </summary>
        /// <param name="musicSyncController"> 音楽同期Controllerです。 </param>
        /// <param name="musicSyncService"> 音楽同期サービスです。 </param>
        /// <param name="musicSyncState"> 音楽同期状態です。 </param>
        public MusicSyncModuleContainer(
            MusicSyncController musicSyncController,
            MusicSyncService musicSyncService,
            MusicSyncState musicSyncState)
        {
            MusicSyncController = musicSyncController;
            MusicSyncService = musicSyncService;
            MusicSyncState = musicSyncState;
        }

        /// <summary> 音楽同期Controllerです。 </summary>
        public MusicSyncController MusicSyncController { get; }

        /// <summary> 音楽同期サービスです。 </summary>
        public MusicSyncService MusicSyncService { get; }

        /// <summary> 音楽同期状態です。 </summary>
        public MusicSyncState MusicSyncState { get; }
    }
}
