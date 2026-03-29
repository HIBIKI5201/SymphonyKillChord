using KillChord.Runtime.Domain;
using System;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    public class EnemyAttackReservationUsecase : IDisposable
    {
        public EnemyAttackReservationUsecase(
            EnemyAttackMusicSpec enemyAttackMusicSpec,
            IMusicActionScheduler musicActionScheduler)
        {
            _enemyAttackMusicSpec = enemyAttackMusicSpec;
            _musicActionScheduler = musicActionScheduler;
        }

        public bool HasReservation => _hasReservation;

        public event Action OnReservedTimingReached;

        public void ReserveEncounter()
        {
            Reserve(_enemyAttackMusicSpec.EncounterTiming);
        }

        public void ReserveBattle()
        {
            Reserve(_enemyAttackMusicSpec.BattleTiming);
        }

        public void Cancel()
        {
            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
            {
                Debug.LogWarning("予約が存在しないか、すでにキャンセルされています。");
                return;
            }

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
            _hasReservation = false;
        }

        public void Dispose()
        {
            _cancellationTokenSource = null;
        }

        private void Reserve(in EnemyMusicSpec musicSpec)
        {
            // 既存の予約をキャンセルしてから新しい予約を設定
            Cancel(); 

            _cancellationTokenSource = new CancellationTokenSource();
            _hasReservation = true;
            
            _musicActionScheduler.Schedule(
                musicSpec,
                HandleReservedTimingReached,
                _cancellationTokenSource.Token);
        }

        private void HandleReservedTimingReached()
        {
            Debug.Log("予約されたタイミングに到達しました。");
            _hasReservation = false;
            OnReservedTimingReached?.Invoke();
        }

        private readonly EnemyAttackMusicSpec _enemyAttackMusicSpec;
        private readonly IMusicActionScheduler _musicActionScheduler;

        private CancellationTokenSource _cancellationTokenSource;
        private bool _hasReservation;
    }
}
