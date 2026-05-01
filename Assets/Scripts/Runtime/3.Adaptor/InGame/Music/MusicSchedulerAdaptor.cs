using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.Persistent.Music;
using System;
using System.Threading;

namespace KillChord.Runtime.Adaptor.InGame.Music
{
    /// <summary>
    ///     音楽同期に合わせたアクションのスケジュールを管理するアダプタークラス。
    /// </summary>
    public class MusicSchedulerAdaptor : IMusicActionScheduler
    {
        public MusicSchedulerAdaptor(MusicSyncState syncState, IMusicSyncService musicSyncService)
        {
            _musicSyncState = syncState;
            _musicSyncService = musicSyncService;
        }

        public void Schedule(in EnemyMusicSpec musicSpec,
            Action action,
            CancellationToken cancellationToken)
        {
            ExecuteRequestTiming timing = Convert(musicSpec);
            double accurateBeat = _musicSyncState.AccurateBeat;

            _musicSyncService.RegisterAction(
                accurateBeat,
                timing,
                action,
                cancellationToken
                );
        }

        private readonly MusicSyncState _musicSyncState;
        private readonly IMusicSyncService _musicSyncService;

        private ExecuteRequestTiming Convert(in EnemyMusicSpec musicSpec)
        {
            Beat beat = new Beat(musicSpec.TimeSignature, musicSpec.TargetBeat);
            return new ExecuteRequestTiming((byte)musicSpec.BarFlag, beat);
        }
    }
}
