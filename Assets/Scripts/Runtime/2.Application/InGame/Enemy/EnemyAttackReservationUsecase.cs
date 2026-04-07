using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Enemy;
using System;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Player
{
    /// <summary>
    ///     敵の攻撃を予約するユースケース。
    /// </summary>
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
            Debug.Log("[EnemyAttackReservationUsecase] ReserveEncounter 呼び出し");
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
                Debug.Log("予約が存在しないか、すでにキャンセルされています。");
                return;
            }

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
            _hasReservation = false;
        }

        public void Dispose()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
            _hasReservation = false;
        }

        private void Reserve(in EnemyMusicSpec musicSpec)
        {
            Debug.Log("[EnemyAttackReservationUsecase] Reserve 開始");
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
