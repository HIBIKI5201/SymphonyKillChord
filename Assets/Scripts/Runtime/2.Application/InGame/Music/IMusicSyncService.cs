using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Music;
using System;
using System.Threading;

namespace KillChord.Runtime.Application.InGame.Music
{
    public interface IMusicSyncService
    {
        void Update(double playTime);
        int GetHistoryLength();

        /// <summary> プレイヤーの入力履歴のうち拍情報を取得する </summary>
        ReadOnlySpan<int> GetBeatTypeHistory();

        /// <summary> プレイヤーの入力履歴のうち、入力したunscaledTimeを取得する </summary>
        ReadOnlySpan<float> GetBeatTypeTiming();

        /// <summary> プレイヤーの入力履歴のうち、アクションの種類を取得する </summary>
        ReadOnlySpan<BattleActionType> GetActionHistory();

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

        /// <summary> 現在のタイミング（Time.unscaledTime）が何拍子相当かを取得する </summary>
        int GetCurrentBeatType();

        /// <summary> プレイヤーの行動履歴を保存する </summary>
        /// <param name="actionType">アクションの種類</param>
        /// <param name="beatType">計算済みの拍子</param>
        void RegisterBattleActionHistory(BattleActionType actionType, int beatType);
    }
}