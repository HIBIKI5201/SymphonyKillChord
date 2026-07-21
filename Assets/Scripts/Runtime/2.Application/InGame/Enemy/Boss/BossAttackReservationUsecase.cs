using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Music;
using System;
using System.Threading;

namespace KillChord.Runtime.Application.InGame.Enemy
{
    /// <summary>
    ///     ボス専用の攻撃予約ユースケース。
    ///     EnemyAttackReservationUsecase には手を入れず、
    ///     IMusicActionScheduler を直接使って任意タイミングの予約を行う。
    /// </summary>
    public sealed class BossAttackReservationUsecase : IDisposable
    {
        public BossAttackReservationUsecase(IMusicActionScheduler musicActionScheduler)
        {
            _musicActionScheduler = musicActionScheduler;
        }

        /// <summary> 予約が存在するか。 </summary>
        public bool HasReservation => _hasReservation;

        /// <summary> 予約タイミング到達時に発火する。 </summary>
        public event Action OnReservedTimingReached;
        /// <summary> 攻撃の2拍前に発火する。 </summary>
        public event Action On2BeatBefore;
        /// <summary> 攻撃の1拍前に発火する。 </summary>
        public event Action On1BeatBefore;

        /// <summary>
        ///     指定タイミングで攻撃を予約する。
        ///     既存予約がある場合はキャンセルしてから設定する。
        /// </summary>
        public void Reserve(in MusicSyncSpec timing)
        {
            Cancel();

            _cancellationTokenSource = new CancellationTokenSource();
            _hasReservation = true;

            _musicActionScheduler.Schedule(
                timing,
                HandleReservedTimingReached,
                _cancellationTokenSource.Token);

            // 2拍前・1拍前の予兆。拍が足りない場合は前小節へ巻き込んでスケジュールする。
            // 例: 3拍子2拍目なら、2拍前は前小節の3拍目（BarFlag-1）になる。
            ScheduleBeatBefore(timing, 2, Handle2BeatBefore);
            ScheduleBeatBefore(timing, 1, Handle1BeatBefore);
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

        /// <summary>
        ///     無効化処理。
        /// </summary>
        public void Deactivate() => Cancel();

        public void Dispose() => Cancel();

        private void HandleReservedTimingReached()
        {
            _hasReservation = false;
            OnReservedTimingReached?.Invoke();
        }

        private void Handle2BeatBefore() => On2BeatBefore?.Invoke();

        private void Handle1BeatBefore() => On1BeatBefore?.Invoke();

        /// <summary>
        ///     攻撃の指定拍数前にコールバックをスケジュールする。
        ///     対象拍が小節頭より前になる場合は、前小節（BarFlag-1）へ巻き込んで拍を補正する。
        /// </summary>
        /// <param name="timing"> 攻撃本体のタイミング。 </param>
        /// <param name="beatsBefore"> 何拍前か（2拍前なら2）。 </param>
        /// <param name="handler"> 到達時に呼ぶ処理。 </param>
        private void ScheduleBeatBefore(in MusicSyncSpec timing, double beatsBefore, Action handler)
        {
            double signature = timing.TimeSignature;
            if (signature <= 0d) return;

            double beat = timing.TargetBeat - beatsBefore;
            int barFlag = timing.BarFlag;

            // 拍が1未満なら、前小節へ繰り上げる。
            while (beat < 1d)
            {
                beat += signature;
                barFlag -= 1;
            }

            // 現在小節より前（BarFlagが負）はスケジュール対象外。
            if (barFlag < 0) return;

            _musicActionScheduler.Schedule(
                new MusicSyncSpec((byte)barFlag, signature, beat),
                handler,
                _cancellationTokenSource.Token);
        }

        private readonly IMusicActionScheduler _musicActionScheduler;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _hasReservation;
    }
}
