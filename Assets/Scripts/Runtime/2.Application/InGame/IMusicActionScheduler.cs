using KillChord.Runtime.Domain;
using System;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Application
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
