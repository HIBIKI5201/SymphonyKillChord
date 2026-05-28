using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Enemy;
using System;
using System.Threading;

namespace DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Battle
{
    /// <summary>
    ///     音楽同期のタイミングで実行するアクションを予約するためのインターフェース。
    /// </summary>
    public interface IMusicActionScheduler
    {
        void Schedule(
            in EnemyMusicSpec musicSpec,
            Action action,
            CancellationToken cancellationToken);
    }
}
