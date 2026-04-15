using DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Battle;
using DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Music;
using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Enemy;
using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Music;
using DevelopProducts.BehaviorGraph.Runtime.Domain.Persistent.Music;
using System;
using System.Threading;
using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.Adaptor
{
    /// <summary>
    ///     音楽同期に合わせたアクションのスケジュールを管理するアダプタークラス。
    /// </summary>
    public class MusicSchedulerAdaptor : IMusicActionScheduler
    {
        public MusicSchedulerAdaptor(IMusicSyncViewModel syncViewModel, IMusicSyncService musicSyncService)
        {
            _syncViewModel = syncViewModel;
            _musicSyncService = musicSyncService;
        }

        public void Schedule(in EnemyMusicSpec musicSpec,
            Action action,
            CancellationToken cancellationToken)
        {
            Debug.Log($"[MusicSchedulerAdaptor] syncViewModel null? {_syncViewModel == null}");
            Debug.Log($"[MusicSchedulerAdaptor] musicSyncService null? {_musicSyncService == null}");

            Debug.Log("攻撃予約: " + musicSpec);
            ExecuteRequestTiming timing = Convert(musicSpec);

            double accurateBeat = _syncViewModel.AccurateBeat;

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
