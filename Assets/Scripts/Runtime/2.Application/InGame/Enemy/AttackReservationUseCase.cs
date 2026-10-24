using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Music;
using System;
using System.Threading;

namespace KillChord.Runtime.Application.InGame.Enemy
{
    /// <summary>
    ///     敵・ボス共通の攻撃予約ユースケース。
    ///     指定タイミングで攻撃を予約し、攻撃の2拍前・1拍前にも通知する。
    /// </summary>
    public class AttackReservationUseCase : IDisposable
    {
        /// <summary>
        ///     拍の予約に使うスケジューラーを指定して生成する。
        /// </summary>
        /// <param name="musicActionScheduler"> 拍の予約に使うスケジューラー。 </param>
        public AttackReservationUseCase(IMusicActionScheduler musicActionScheduler)
        {
            _musicActionScheduler = musicActionScheduler;
        }

        /// <summary> 予約タイミング到達時に発火する。 </summary>
        public event Action OnReservedTimingReached;
        /// <summary> 攻撃の2拍前に発火する。 </summary>
        public event Action On2BeatBefore;
        /// <summary> 攻撃の1拍前に発火する。 </summary>
        public event Action On1BeatBefore;

        /// <summary> 予約が存在するか。 </summary>
        public bool HasReservation => _hasReservation;

        /// <summary> 予約中の攻撃時刻（音源再生時間・秒）。予約が無い場合は無効。 </summary>
        public double AttackExecutionTime { get; private set; }

        /// <summary>
        ///     指定タイミングで攻撃を予約する。
        ///     既存予約がある場合はキャンセルしてから設定する。
        /// </summary>
        /// <param name="timing"> 攻撃本体のタイミング。 </param>
        public void Reserve(in MusicSyncSpec timing)
        {
            CancellationToken token = _cancellationSource.Renew();
            _hasReservation = true;

            // 攻撃の絶対時刻を保持し、演出側が残り時間から進捗を算出できるようにする。
            AttackExecutionTime = _musicActionScheduler.Schedule(
                timing,
                HandleReservedTimingReached,
                token);

            // 2拍前・1拍前の予兆。拍が足りない場合は前小節へ巻き込んでスケジュールする。
            LeadNotificationScheduler.TrySchedule(_musicActionScheduler, timing, TWO_BEAT_LEAD, Handle2BeatBefore, token);
            LeadNotificationScheduler.TrySchedule(_musicActionScheduler, timing, ONE_BEAT_LEAD, Handle1BeatBefore, token);
        }

        /// <summary>
        ///     予約をキャンセルする。
        /// </summary>
        public void Cancel()
        {
            _cancellationSource.Cancel();
            _hasReservation = false;
        }

        /// <summary>
        ///     無効化処理。
        /// </summary>
        public void Deactivate()
        {
            Cancel();
        }

        /// <summary>
        ///     予約中の攻撃をキャンセルして破棄する。
        /// </summary>
        public void Dispose()
        {
            Cancel();
        }

        /// <summary> 2拍前の予告に使う遡り量。 </summary>
        private const double TWO_BEAT_LEAD = 2d;
        /// <summary> 1拍前の予告に使う遡り量。 </summary>
        private const double ONE_BEAT_LEAD = 1d;

        private readonly IMusicActionScheduler _musicActionScheduler;
        private readonly ReservationCancellationSource _cancellationSource = new ReservationCancellationSource();
        private bool _hasReservation;

        /// <summary>
        ///     予約した拍に到達したとき、予約状態を解除して通知する。
        /// </summary>
        private void HandleReservedTimingReached()
        {
            _hasReservation = false;
            OnReservedTimingReached?.Invoke();
        }

        /// <summary>
        ///     攻撃の2拍前になったことを通知する。
        /// </summary>
        private void Handle2BeatBefore()
        {
            On2BeatBefore?.Invoke();
        }

        /// <summary>
        ///     攻撃の1拍前になったことを通知する。
        /// </summary>
        private void Handle1BeatBefore()
        {
            On1BeatBefore?.Invoke();
        }
    }
}
