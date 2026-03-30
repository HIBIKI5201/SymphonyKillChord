using KillChord.Runtime.Domain;
using UnityEngine;
using System;
using System.Threading;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain.Persistent.Music;

namespace KillChord.Runtime.Adaptor
{
    public class MusicSchedulerAdaptor : IMusicActionScheduler
    {
        public MusicSchedulerAdaptor(IMusicSyncViewModel syncViewModel,IMusicSyncService musicSyncService)
        {
            _syncViewModel = syncViewModel;
            _musicSyncService = musicSyncService;
        }

        public void Schedule(in EnemyMusicSpec musicSpec,
            Action action,
            CancellationToken cancellationToken)
        {
            Debug.Log("攻撃予約: " + musicSpec);
            ExecuteRequestTiming timing = Convert(musicSpec);

            double accurateBeat =_syncViewModel.AccurateBeat;

            _musicSyncService.RegisterAction(
                accurateBeat,
                timing,
                action,
                cancellationToken
                );
        }

        private ExecuteRequestTiming Convert(in EnemyMusicSpec musicSpec)
        {
            Beat beat = new Beat(musicSpec.TimeSignature, musicSpec.TargetBeat);
            return new ExecuteRequestTiming((byte)musicSpec.BarFlag, beat);
        }

        private readonly IMusicSyncViewModel _syncViewModel;
        private readonly IMusicSyncService _musicSyncService;
    }
}
