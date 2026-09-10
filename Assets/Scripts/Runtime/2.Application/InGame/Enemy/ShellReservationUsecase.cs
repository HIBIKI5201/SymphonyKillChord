using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Music;
using System;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Enemy
{
    /// <summary>
    ///     砲弾の爆発予約処理。
    /// </summary>
    public class ShellReservationUsecase : IDisposable
    {
        public ShellReservationUsecase(ShellEntity entity, IMusicActionScheduler musicActionScheduler)
        {
            if(entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "ShellEntityがNULLです。");
            }
            _entity = entity;
            _musicActionScheduler = musicActionScheduler;
        }

        /// <summary> 予約タイミングが到達した時発火するイベント </summary>
        public event Action OnReservedTimingReached;
        /// <summary> 着弾予告（デカールの変化開始）タイミングが到達した時発火するイベント </summary>
        public event Action OnAreaWarning;

        /// <summary> 予約中の爆発時刻（音源再生時間・秒）。予約が無い場合は無効。 </summary>
        public double DetonateExecutionTime { get; private set; }
        /// <summary> 爆発予約が有効かどうか。 </summary>
        public bool HasDetonateReservation { get; private set; }

        public void Dispose()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            HasDetonateReservation = false;
        }
        /// <summary>
        ///     予約をキャンセルする。
        /// </summary>
        public void Cancel()
        {
            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
            {
                Debug.Log("予約が存在しないか、すでにキャンセルされています。");
                return;
            }

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
            HasDetonateReservation = false;
        }
        /// <summary>
        ///     爆発タイミングを予約する。
        /// </summary>
        public void ReserveDetonate()
        {
            // 既存の予約をキャンセルしてから新しい予約を設定する。
            Cancel();

            _cancellationTokenSource = new CancellationTokenSource();

            // 爆発の絶対時刻を保持し、演出側が残り時間から進捗を算出できるようにする。
            DetonateExecutionTime = _musicActionScheduler.Schedule(
                _entity.MusicSpec,
                HandleReservedTimingReached,
                _cancellationTokenSource.Token);
            HasDetonateReservation = true;

            ScheduleAreaWarning(_entity.MusicSpec, _cancellationTokenSource.Token);
        }

        /// <summary>
        ///     着弾予告（デカールの変化開始）タイミングを予約する。
        ///     デカール側の進捗演出（ShellLifeCycle.GetDetonateApproach）が0から動き出す瞬間と
        ///     完全に同じ拍になるよう、爆発予約と同じ拍数（ShellMusicConstants.DETONATE_LEAD_BEAT_COUNT）
        ///     だけ遡ったタイミングを使う。
        /// </summary>
        /// <param name="musicSpec"> 爆発本体のタイミング。 </param>
        /// <param name="token"> キャンセルトークン。 </param>
        private void ScheduleAreaWarning(in MusicSyncSpec musicSpec, CancellationToken token)
        {
            if (!MusicTimingCalculator.TryCreateLeadTiming(musicSpec, ShellMusicConstants.DETONATE_LEAD_BEAT_COUNT, out MusicSyncSpec leadSpec))
            {
                return;
            }

            _musicActionScheduler.Schedule(leadSpec, HandleAreaWarning, token);
        }

        /// <summary>
        ///     予約タイミングが到達時の処理。
        /// </summary>
        private void HandleReservedTimingReached()
        {
            Debug.Log("予約されたタイミングに到達しました。");
            HasDetonateReservation = false;
            OnReservedTimingReached?.Invoke();
        }

        /// <summary>
        ///     着弾予告（デカールの変化開始）タイミングが到達時の処理。
        /// </summary>
        private void HandleAreaWarning()
        {
            Debug.Log("着弾予告タイミングに到達しました。");
            OnAreaWarning?.Invoke();
        }

        private readonly ShellEntity _entity;
        private readonly IMusicActionScheduler _musicActionScheduler;
        private CancellationTokenSource _cancellationTokenSource;
    }
}
