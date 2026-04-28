using KillChord.Runtime.Application.InGame.Battle;
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
        public ShellReservationUsecase(IMusicActionScheduler musicActionScheduler)
        {
            _musicActionScheduler = musicActionScheduler;
            // TODO 砲弾の爆発タイミング。今後ScriptableObjectか、敵の攻撃情報から初期化する予想。
            _musicSpec = new EnemyMusicSpec(1, 4, 3);
        }

        public event Action OnReservedTimingReached;

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
                _musicSpec,
                HandleReservedTimingReached,
                _cancellationTokenSource.Token);
        }

        private void HandleReservedTimingReached()
        {
            Debug.Log("予約されたタイミングに到達しました。");
            OnReservedTimingReached?.Invoke();
        }

        private readonly IMusicActionScheduler _musicActionScheduler;
        private CancellationTokenSource _cancellationTokenSource;
        private EnemyMusicSpec _musicSpec;

    }
}
