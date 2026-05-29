using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Enemy;
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

        public void Dispose()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
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
        }
        /// <summary>
        ///     爆発タイミングを予約する。
        /// </summary>
        public void ReserveDetonate()
        {
            // 既存の予約をキャンセルしてから新しい予約を設定する。
            Cancel();

            _cancellationTokenSource = new CancellationTokenSource();

            _musicActionScheduler.Schedule(
                _entity.MusicSpec,
                HandleReservedTimingReached,
                _cancellationTokenSource.Token);
                _musicActionScheduler.Schedule(
                new EnemyMusicSpec(_entity.MusicSpec.BarFlag, _entity.MusicSpec.TimeSignature, _entity.MusicSpec.TargetBeat - 2),// 2拍前
                HandleReservedTimingReached,
                _cancellationTokenSource.Token);
                _musicActionScheduler.Schedule(
                new EnemyMusicSpec(_entity.MusicSpec.BarFlag, _entity.MusicSpec.TimeSignature, _entity.MusicSpec.TargetBeat - 1),// 1拍前
                HandleReservedTimingReached,
                _cancellationTokenSource.Token);
        }

        /// <summary>
        ///     予約タイミングが到達時の処理。
        /// </summary>
        private void HandleReservedTimingReached()
        {
            Debug.Log("予約されたタイミングに到達しました。");
            OnReservedTimingReached?.Invoke();
        }

        /// <summary>
        ///    予約タイミングが到達の2拍前の処理。
        /// </summary>
        private void Handle2BeatBefore()
        {
            Debug.Log("[ShellReservationUsecase] 爆発の2拍前");
        }

        /// <summary>
        ///    予約タイミングが到達の1拍前の処理。
        /// </summary>
        private void Handle1BeatBefore()
        {
            Debug.Log("[ShellReservationUsecase] 爆発の1拍前");
        }


        private readonly ShellEntity _entity;
        private readonly IMusicActionScheduler _musicActionScheduler;
        private CancellationTokenSource _cancellationTokenSource;
    }
}
