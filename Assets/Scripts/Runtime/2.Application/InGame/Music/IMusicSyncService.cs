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
        ///     小節内の進捗を取得する。
        /// </summary>
        /// <param name="unscaledTime"> 現在の時間。 </param>
        /// <returns> 0〜1の進捗。 </returns>
        float GetBarProgress(float unscaledTime);

        /// <summary>
        ///     拍の種類履歴を取得する。
        /// </summary>
        /// <returns> 拍の種類スパン。 </returns>
        ReadOnlySpan<BeatType> GetBeatTypeHistory();

        /// <summary>
        ///     拍のタイミング履歴を取得する。
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
        void RegisterAction(
            double accurateBeat,
            ExecuteRequestTiming timing,
            Action action,
            CancellationToken ct);

        /// <summary>
        ///     現在の拍の種類を取得する。
        /// </summary>
        /// <param name="unscaledTime"> 現在の時間。 </param>
        /// <returns> 拍の種類。 </returns>
        BeatType GetCurrentBeatType(float unscaledTime);

        /// <summary>
        ///     アクション履歴を登録する。
        /// </summary>
        /// <param name="actionType"> アクションの種類。 </param>
        /// <param name="beatType"> 拍の種類。 </param>
        /// <param name="unscaledTime"> 登録時間。 </param>
        void RegisterBattleActionHistory(BattleActionType actionType, BeatType beatType, float unscaledTime);
    }
}