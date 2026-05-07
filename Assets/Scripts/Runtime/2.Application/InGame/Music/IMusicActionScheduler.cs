using KillChord.Runtime.Domain.InGame.Enemy;
using System;
using System.Threading;

namespace KillChord.Runtime.Application.InGame.Battle
{
    /// <summary>
    ///     音楽同期のタイミングで実行するアクションを予約するためのインターフェース。
    /// </summary>
    public interface IMusicActionScheduler
    {
        /// <summary>
        ///     アクションをスケジュールする。
        /// </summary>
        /// <param name="musicSpec"> 敵の音楽スペック。 </param>
        /// <param name="action"> 実行するアクション。 </param>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        void Schedule(
            in EnemyMusicSpec musicSpec,
            Action action,
            CancellationToken cancellationToken);
    }
}
