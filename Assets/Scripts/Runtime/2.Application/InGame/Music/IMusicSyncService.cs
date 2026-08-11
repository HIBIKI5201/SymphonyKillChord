using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Music;
using System;
using System.Threading;

namespace KillChord.Runtime.Application.InGame.Music
{
    /// <summary>
    ///     音楽同期およびアクション予約を管理するサービスのインターフェース。
    /// </summary>
    public interface IMusicSyncService
    {
        /// <summary>
        ///     更新処理を行い、予約されたアクションを実行する。
        /// </summary>
        /// <param name="playTime"> 再生時間。 </param>
        void Update(double playTime);

        /// <summary>
        ///     履歴の長さを取得する。
        /// </summary>
        /// <returns> 履歴の数。 </returns>
        int GetHistoryLength();

        /// <summary>
        ///     直前のアクション入力から次の小節までの進捗を取得する。
        /// </summary>
        /// <returns> 0〜1の進捗。 </returns>
        float GetBarProgress();

        /// <summary>
        ///     拍の種類履歴を取得する。
        /// </summary>
        /// <returns> 拍の種類スパン。 </returns>
        ReadOnlySpan<BeatType> GetBeatTypeHistory();

        /// <summary>
        ///     音源再生時間を基準とした拍のタイミング履歴を取得する。
        /// </summary>
        /// <returns> タイミングスパン。 </returns>
        ReadOnlySpan<float> GetBeatTypeTiming();

        /// <summary>
        ///     アクション履歴を取得する。
        /// </summary>
        /// <returns> アクション種類スパン。 </returns>
        ReadOnlySpan<BattleActionType> GetActionHistory();

        /// <summary>
        ///     将来実行するアクションを予約する。
        /// </summary>
        /// <param name="accurateBeat"> 正確な拍。 </param>
        /// <param name="timing"> 実行タイミング。 </param>
        /// <param name="action"> 実行アクション。 </param>
        /// <param name="ct"> キャンセルトークン。 </param>
        /// <returns> 実行される音源再生時間（秒）。 </returns>
        double RegisterAction(
            double accurateBeat,
            ExecuteRequestTiming timing,
            Action action,
            CancellationToken ct);

        /// <summary>
        ///     現在の拍の種類を取得する。
        /// </summary>
        /// <returns> 拍の種類。 </returns>
        BeatType GetCurrentBeatType();

        /// <summary>
        ///     アクション履歴を登録する。
        /// </summary>
        /// <param name="actionType"> アクションの種類。 </param>
        /// <param name="beatType"> 拍の種類。 </param>
        void RegisterBattleActionHistory(BattleActionType actionType, BeatType beatType);
    }
}
