using KillChord.Runtime.Domain.InGame.Music;
using System;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Music
{
    /// <summary>
    ///     基準タイミングから指定拍数だけ遡った予告を予約するための共通処理。
    ///     EnemyAttackReservationUsecase（敵の攻撃予告）とShellReservationUsecase
    ///     （砲弾の着弾予告）の両方から利用される。
    /// </summary>
    public static class LeadNotificationScheduler
    {
        /// <summary>
        ///     基準タイミングから指定拍だけ遡った予告を予約する。
        ///     遡った結果が小節の頭を跨ぐ場合は、小節フラグを繰り下げて前の小節へ割り当てる。
        /// </summary>
        /// <param name="scheduler"> 予約に使用するスケジューラー。 </param>
        /// <param name="musicSpec"> 基準タイミング。 </param>
        /// <param name="leadBeatCount"> 遡る量。拍子と同じ単位で指定する。 </param>
        /// <param name="handler"> 予告タイミングで実行する処理。 </param>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 予告タイミングを予約できた場合はtrue。遡り量が基準タイミングより大きく予約できなかった場合はfalse。 </returns>
        public static bool TrySchedule(
            IMusicActionScheduler scheduler,
            in MusicSyncSpec musicSpec,
            double leadBeatCount,
            Action handler,
            CancellationToken cancellationToken)
        {
            if (!MusicTimingCalculator.TryCreateLeadTiming(musicSpec, leadBeatCount, out MusicSyncSpec leadSpec))
            {
                // 遺り量が基準タイミングより大きく、予告が静かに発火しなくなることを検知できるようにする。
                Debug.LogWarning($"[{nameof(LeadNotificationScheduler)}] 予告タイミングの算出に失敗しました。leadBeatCount: {leadBeatCount}");
                return false;
            }

            scheduler.Schedule(leadSpec, handler, cancellationToken);
            return true;
        }
    }
}
