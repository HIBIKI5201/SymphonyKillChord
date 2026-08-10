using KillChord.Runtime.Domain.InGame.Music;
using System;
using System.Threading;

namespace KillChord.Runtime.Application.InGame.Music
{
    /// <summary>
    ///     音楽同期のタイミングで実行するアクションを予約するためのインターフェース。
    /// </summary>
    public interface IMusicActionScheduler
    {
        /// <summary>
        ///     アクションをスケジュールする。
        /// </summary>
        /// <param name="musicSpec"> 音楽同期スペック。 </param>
        /// <param name="action"> 実行するアクション。 </param>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 実行される音源再生時間（秒）。 </returns>
        double Schedule(
            in MusicSyncSpec musicSpec,
            Action action,
            CancellationToken cancellationToken);
    }
}
