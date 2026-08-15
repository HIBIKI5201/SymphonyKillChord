using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Music;
using System;
using System.Threading;
using UnityEngine;

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
            Debug.Log("[EnemyAttackReservationUsecase] ReserveEncounter 呼び出し");
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
            Debug.Log("[EnemyAttackReservationUsecase] Reserve 開始");
            // 既存の予約をキャンセルしてから新しい予約を設定する。
            Cancel();

            _cancellationTokenSource = new CancellationTokenSource();
            _hasReservation = true;

            // 攻撃の絶対時刻を保持し、演出側が残り時間から進捗を算出できるようにする。
            AttackExecutionTime = _musicActionScheduler.Schedule(
                musicSpec,
                HandleReservedTimingReached,
                _cancellationTokenSource.Token);

            ScheduleLeadNotification(musicSpec, TWO_BEAT_LEAD, Handle2BeatBefore);
            ScheduleLeadNotification(musicSpec, ONE_BEAT_LEAD, Handle1BeatBefore);
        }

        /// <summary>
        ///     攻撃タイミングから指定拍だけ遡った予告を予約する。
        ///     遡った結果が小節の頭を跨ぐ場合は、小節フラグを繰り下げて前の小節へ割り当てる。
        /// </summary>
        /// <param name="musicSpec"> 攻撃本体のタイミング。 </param>
        /// <param name="leadCount"> 遡る量。拍子と同じ単位で指定する。 </param>
        /// <param name="handler"> 予告タイミングで実行する処理。 </param>
        private void ScheduleLeadNotification(in MusicSyncSpec musicSpec, double leadCount, Action handler)
        {
            if (!MusicTimingCalculator.TryCreateLeadTiming(musicSpec, leadCount, out MusicSyncSpec leadSpec))
            {
                return;
            }

            _musicActionScheduler.Schedule(
                leadSpec,
                handler,
                _cancellationTokenSource.Token);
        }

        /// <summary>
        ///     予約タイミングが到達時の処理。
        /// </summary>
        private void HandleReservedTimingReached()
        {
            Debug.Log("予約されたタイミングに到達しました。");
            _hasReservation = false;
            OnReservedTimingReached?.Invoke();
        }

        /// <summary>
        ///    攻撃の2拍前に到達したときの処理。
        /// </summary>
        private void Handle2BeatBefore()
        {
            Debug.Log("攻撃の2拍前に到達しました。");
            On2BeatBefore?.Invoke();
        }
        /// <summary>
        ///   攻撃の1拍前に到達したときの処理。
        /// </summary>
        private void Handle1BeatBefore()
        {
            Debug.Log("攻撃の1拍前に到達しました。");
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
