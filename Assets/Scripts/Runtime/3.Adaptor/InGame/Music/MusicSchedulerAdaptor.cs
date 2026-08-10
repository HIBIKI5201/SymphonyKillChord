using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Application.InGame.Music;
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
        /// <summary>
        ///     新しいアダプターを生成する。
        /// </summary>
        /// <param name="syncState"> 音楽同期状態。 </param>
        /// <param name="musicSyncService"> 音楽同期サービス。 </param>
        public MusicSchedulerAdaptor(MusicSyncState syncState, IMusicSyncService musicSyncService)
        {
            _musicSyncState = syncState;
            _musicSyncService = musicSyncService;
        }

        /// <summary>
        ///     アクションをスケジュールする。
        /// </summary>
        /// <param name="musicSpec"> 音楽同期スペック。 </param>
        /// <param name="action"> 実行するアクション。 </param>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 実行される音源再生時間（秒）。 </returns>
        public double Schedule(in MusicSyncSpec musicSpec,
            Action action,
            CancellationToken cancellationToken)
        {
            ExecuteRequestTiming timing = Convert(musicSpec);
            double accurateBeat = _musicSyncState.AccurateBeat;

            return _musicSyncService.RegisterAction(
                accurateBeat,
                timing,
                action,
                cancellationToken
                );
        }

        private readonly MusicSyncState _musicSyncState;
        private readonly IMusicSyncService _musicSyncService;

        /// <summary>
        ///     MusicSyncSpecをExecuteRequestTimingに変換する。
        /// </summary>
        /// <param name="musicSpec"> 変換元のスペック。 </param>
        /// <returns> 変換後のタイミング情報。 </returns>
        private ExecuteRequestTiming Convert(in MusicSyncSpec musicSpec)
        {
            Beat beat = new Beat(musicSpec.TimeSignature, musicSpec.TargetBeat);
            return new ExecuteRequestTiming((byte)musicSpec.BarFlag, beat);
        }
    }
}
