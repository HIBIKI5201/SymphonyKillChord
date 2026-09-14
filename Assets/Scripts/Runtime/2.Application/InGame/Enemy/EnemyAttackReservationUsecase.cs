using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Music;
using System;
using System.Threading;

namespace KillChord.Runtime.Application.InGame.Enemy
{
    /// <summary>
    ///     敵の攻撃を予約するユースケース。
    /// </summary>
    public class EnemyAttackReservationUsecase : IDisposable
    {
        /// <summary>
        ///     敵の攻撃を予約するユースケースのコンストラクタ。
        /// </summary>
        /// <param name="enemyAttackMusicSpec"></param>
        /// <param name="musicActionScheduler"></param>
        public EnemyAttackReservationUsecase(
            EnemyAttackMusicSpec enemyAttackMusicSpec,
            IMusicActionScheduler musicActionScheduler
            )
        {
            _enemyAttackMusicSpec = enemyAttackMusicSpec;
            _musicActionScheduler = musicActionScheduler;
        }

        /// <summary> 予約が存在するかどうかを示すプロパティ。 </summary>
        public bool HasReservation => _hasReservation;

        /// <summary> 予約中の攻撃時刻（音源再生時間・秒）。予約が無い場合は無効。 </summary>
        public double AttackExecutionTime { get; private set; }

        /// <summary> 予約タイミングが到達時に発火するイベント </summary>
        public event Action OnReservedTimingReached;
        public event Action On2BeatBefore;
        public event Action On1BeatBefore;

        /// <summary>
        ///     Encounterタイミングで攻撃を予約する。
        /// </summary>
        public void ReserveEncounter()
        {
            Reserve(_enemyAttackMusicSpec.EncounterTiming);
        }

        /// <summary>
        ///     Battleタイミングで攻撃を予約する。
        /// </summary>
        public void ReserveBattle()
        {
            Reserve(_enemyAttackMusicSpec.BattleTiming);
        }

        /// <summary>
        ///     予約をキャンセルする。
        /// </summary>
        public void Cancel()
        {
            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
            {
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

        /// <summary>
        ///     無効化処理。
        /// </summary>
        public void Deactivate()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
            _hasReservation = false;
        }

        /// <summary>
        ///     予約を設定する内部メソッド。
        ///     既存の予約がある場合はキャンセルしてから新しい予約を設定する。
        /// </summary>
        /// <param name="musicSpec"></param>
        private void Reserve(in MusicSyncSpec musicSpec)
        {
            // 既存の予約をキャンセルしてから新しい予約を設定する。
            Cancel();

            _cancellationTokenSource = new CancellationTokenSource();
            _hasReservation = true;

            // 攻撃の絶対時刻を保持し、演出側が残り時間から進捗を算出できるようにする。
            AttackExecutionTime = _musicActionScheduler.Schedule(
                musicSpec,
                HandleReservedTimingReached,
                _cancellationTokenSource.Token);

            LeadNotificationScheduler.TrySchedule(_musicActionScheduler, musicSpec, TWO_BEAT_LEAD, Handle2BeatBefore, _cancellationTokenSource.Token);
            LeadNotificationScheduler.TrySchedule(_musicActionScheduler, musicSpec, ONE_BEAT_LEAD, Handle1BeatBefore, _cancellationTokenSource.Token);
        }

        /// <summary>
        ///     予約タイミングが到達時の処理。
        /// </summary>
        private void HandleReservedTimingReached()
        {
            _hasReservation = false;
            OnReservedTimingReached?.Invoke();
        }

        /// <summary>
        ///    攻撃の2拍前に到達したときの処理。
        /// </summary>
        private void Handle2BeatBefore()
        {
            On2BeatBefore?.Invoke();
        }
        /// <summary>
        ///   攻撃の1拍前に到達したときの処理。
        /// </summary>
        private void Handle1BeatBefore()
        {
            On1BeatBefore?.Invoke();
        }


        /// <summary> 2拍前の予告に使う遡り量。 </summary>
        private const double TWO_BEAT_LEAD = 2d;
        /// <summary> 1拍前の予告に使う遡り量。 </summary>
        private const double ONE_BEAT_LEAD = 1d;

        private readonly EnemyAttackMusicSpec _enemyAttackMusicSpec;
        private readonly IMusicActionScheduler _musicActionScheduler;

        private CancellationTokenSource _cancellationTokenSource;
        private bool _hasReservation;
    }
}
