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
        /// <summary>
        ///   予約タイミングの2拍前に発火するイベント
        /// </summary>
        public event Action On2BeatBefore;
        /// <summary>
        /// 予約タイミングの1拍前に発火するイベント
        /// </summary>
        public event Action On1BeatBefore;

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
                _musicActionScheduler.Schedule(
                new MusicSyncSpec(_entity.MusicSpec.BarFlag, _entity.MusicSpec.TimeSignature, _entity.MusicSpec.TargetBeat - 2),// 2拍前
                Handle2BeatBefore,
                _cancellationTokenSource.Token);
                _musicActionScheduler.Schedule(
                new MusicSyncSpec(_entity.MusicSpec.BarFlag, _entity.MusicSpec.TimeSignature, _entity.MusicSpec.TargetBeat - 1),// 1拍前
                Handle1BeatBefore,
                _cancellationTokenSource.Token);
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
        ///    予約タイミングが到達の2拍前の処理。
        /// </summary>
        private void Handle2BeatBefore()
        {
            Debug.Log("[ShellReservationUsecase] 爆発の2拍前");
            On2BeatBefore?.Invoke();
        }

        /// <summary>
        ///    予約タイミングが到達の1拍前の処理。
        /// </summary>
        private void Handle1BeatBefore()
        {
            Debug.Log("[ShellReservationUsecase] 爆発の1拍前");
            On1BeatBefore?.Invoke();
        }


        private readonly ShellEntity _entity;
        private readonly IMusicActionScheduler _musicActionScheduler;
        private CancellationTokenSource _cancellationTokenSource;
    }
}
