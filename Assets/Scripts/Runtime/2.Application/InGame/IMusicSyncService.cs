using System;
using System.Threading;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    public interface IMusicSyncService
    {
        void Update(double playTime);
        int GetHistoryLength();

        /// <summary> プレイヤーの入力履歴のうち拍情報を取得する </summary>
        ReadOnlySpan<int> GetBeatTypeHistory();

        /// <summary> プレイヤーの入力履歴のうち、入力したunscaledTimeを保存する </summary>
        ReadOnlySpan<float> GetBeatTypeTiming();

        /// <summary> プレイヤーの入力履歴のうち、アクションの種類を保存する </summary>
        ReadOnlySpan<ActionType> GetActionHistory();

        /// <summary>
        /// メソッドの実行予約をする
        /// </summary>
        /// <param name="accurateBeat">IMusicSyncViewModelのAccurateBeatを利用する</param>
        /// <param name="timing"></param>
        /// <param name="action"></param>
        /// <param name="ct"></param>
        void RegisterAction(
            double accurateBeat,
            ExecuteRequestTiming timing,
            Action action,
            CancellationToken ct);

        /// <summary> プレイヤーの行動履歴を保存する </summary>
        void RegisterBattleActionHistory(ActionType actionType);
    }
}