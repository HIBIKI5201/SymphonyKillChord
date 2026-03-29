using System;
using System.Threading;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    public interface IMusicSyncService
    {
        void Update(double playTime);

        /// <summary> メソッドの実行予約を行う </summary>
        void RegisterAction(double accurateBeat,
            ExecuteRequestTiming timing,
            Action action,
            CancellationToken ct);

        /// <summary> プレイヤーの行動履歴を保存する </summary>
        void RegisterButtleActionHistory(ActionType actionType);
    }
}